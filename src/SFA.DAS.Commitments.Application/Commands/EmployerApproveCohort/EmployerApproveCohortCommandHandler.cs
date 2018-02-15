using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.Commands.EmployerApproveCohort
{
    public sealed class EmployerApproveCohortCommandHandler : AsyncRequestHandler<EmployerApproveCohortCommand>
    {
        private readonly AbstractValidator<EmployerApproveCohortCommand> _validator;
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly ICurrentDateTime _currentDateTime;
        private readonly OverlappingApprenticeshipService _overlappingApprenticeshipService;
        private readonly HistoryService _historyService;
        private readonly ApprenticeshipEventsService _apprenticeshipEventsService;

        public EmployerApproveCohortCommandHandler(AbstractValidator<EmployerApproveCohortCommand> validator, ICommitmentRepository commitmentRepository, IApprenticeshipRepository apprenticeshipRepository, IApprenticeshipOverlapRules overlapRules, ICurrentDateTime currentDateTime, IHistoryRepository historyRepository, IApprenticeshipEventsList apprenticeshipEventsList, IApprenticeshipEventsPublisher apprenticeshipEventsPublisher)
        {
            _validator = validator;
            _commitmentRepository = commitmentRepository;
            _apprenticeshipRepository = apprenticeshipRepository;
            _currentDateTime = currentDateTime;
            _overlappingApprenticeshipService = new OverlappingApprenticeshipService(apprenticeshipRepository, overlapRules);
            _historyService = new HistoryService(historyRepository);
            _apprenticeshipEventsService = new ApprenticeshipEventsService(apprenticeshipEventsList, apprenticeshipEventsPublisher, _apprenticeshipRepository);
        }

        protected override async Task HandleCore(EmployerApproveCohortCommand message)
        {
            _validator.ValidateAndThrow(message);

            var commitment = await GetCommitment(message.CommitmentId);
            await CheckCommitmentCanBeApproved(commitment, message.Caller.Id);

            var isFinalApproval = IsFinalApproval(commitment);
            await UpdateApprenticeships(commitment, isFinalApproval);
            await UpdateCommitment(commitment, isFinalApproval, message.UserId, message.LastUpdatedByName, message.LastUpdatedByEmail, message.Message);
            await PublishApprenticeshipEvents(commitment, isFinalApproval);
        }

        private async Task PublishApprenticeshipEvents(Commitment commitment, bool isFinalApproval)
        {
            if (!isFinalApproval)
            {
                await _apprenticeshipEventsService.PublishApprenticeshipAgreementUpdatedEvents(commitment);
            }
            else
            {
                await _apprenticeshipEventsService.PublishApprenticeshipFinalApprovalEvents(commitment);
            }
        }

        private async Task UpdateCommitment(Commitment commitment, bool isFinalApproval, string userId, string lastUpdatedByName, string lastUpdatedByEmail, string message)
        {
            var updatedEditStatus = DetermineNewEditStatus(isFinalApproval);
            var changeType = DetermineHistoryChangeType(isFinalApproval);
            _historyService.TrackUpdate(commitment, changeType.ToString(), commitment.Id, null, CallerType.Employer, userId, commitment.ProviderId, commitment.EmployerAccountId, lastUpdatedByName);

            commitment.EditStatus = updatedEditStatus;
            commitment.LastAction = LastAction.Approve;
            commitment.LastUpdatedByEmployerEmail = lastUpdatedByEmail;
            commitment.LastUpdatedByEmployerName = lastUpdatedByName;

            await Task.WhenAll(
                AddMessageToCommitment(commitment, lastUpdatedByName, message),
                _commitmentRepository.UpdateCommitment(commitment), 
                _historyService.Save()
            );
        }

        private async Task AddMessageToCommitment(Commitment commitment, string lastUpdatedByName, string messageText)
        {
            var cohortStatusChangeService = new CohortStatusChangeService(_commitmentRepository);
            await cohortStatusChangeService.AddMessageToCommitment(commitment, lastUpdatedByName, messageText);
        }

        private CommitmentChangeType DetermineHistoryChangeType(bool isFinalApproval)
        {
            return isFinalApproval ? CommitmentChangeType.FinalApproval : CommitmentChangeType.SentForApproval;
        }

        private EditStatus DetermineNewEditStatus(bool isFinalApproval)
        {
            return isFinalApproval ? EditStatus.Both : EditStatus.ProviderOnly;
        }

        private bool IsFinalApproval(Commitment commitment)
        {
            var currentAgreementStatus = GetCurrentAgreementStatus(commitment);
            return currentAgreementStatus == AgreementStatus.ProviderAgreed;
        }

        private async Task UpdateApprenticeships(Commitment commitment, bool isFinalApproval)
        {
            UpdateApprenticeshipStatuses(commitment, isFinalApproval);
            await _apprenticeshipRepository.UpdateApprenticeshipStatuses(commitment.Apprenticeships);
            if (isFinalApproval)
            {
                await CreatePriceHistory(commitment);
            }
        }

        private async Task CreatePriceHistory(Commitment commitment)
        {
            await _apprenticeshipRepository.CreatePriceHistoryForApprenticeshipsInCommitment(commitment.Id);

            foreach (var apprenticeship in commitment.Apprenticeships)
            {
                apprenticeship.PriceHistory = new List<PriceHistory>
                {
                    new PriceHistory { ApprenticeshipId = apprenticeship.Id, Cost = apprenticeship.Cost.Value, FromDate = apprenticeship.StartDate.Value }
                };
            }
        }

        private void UpdateApprenticeshipStatuses(Commitment commitment, bool isFinalApproval)
        {
            var newAgreementStatus = DetermineNewAgreementStatus(isFinalApproval);
            var newPaymentStatus = DetermineNewPaymentStatus(isFinalApproval);
            commitment.Apprenticeships.ForEach(x =>
            {
                x.AgreementStatus = newAgreementStatus;
                x.PaymentStatus = newPaymentStatus;
                x.AgreedOn = DetermineAgreedOnDate(isFinalApproval);
            });
        }

        private DateTime? DetermineAgreedOnDate(bool isFinalApproval)
        {
            return isFinalApproval ? _currentDateTime.Now : (DateTime?)null;
        }

        private static PaymentStatus DetermineNewPaymentStatus(bool isFinalApproval)
        {
            return isFinalApproval ? PaymentStatus.Active : PaymentStatus.PendingApproval;
        }

        private static AgreementStatus DetermineNewAgreementStatus(bool isFinalApproval)
        {
            var newAgreementStatus = isFinalApproval ? AgreementStatus.BothAgreed : AgreementStatus.EmployerAgreed;
            return newAgreementStatus;
        }

        private static AgreementStatus GetCurrentAgreementStatus(Commitment commitment)
        {
            // Comment 1: This assumes, correctly, that during approval all apprenticeships have the same status.
            // Comment 2: I hate comments.
            return commitment.Apprenticeships.First().AgreementStatus;
        }

        private async Task CheckCommitmentCanBeApproved(Commitment commitment, long callerEmployerAccountId)
        {
            CheckCommitmentStatus(commitment);
            CheckEditStatus(commitment);
            CheckAuthorization(callerEmployerAccountId, commitment);
            CheckStateForApproval(commitment);
            await CheckOverlaps(commitment);
        }

        private async Task CheckOverlaps(Commitment commitment)
        {
            if (await _overlappingApprenticeshipService.CommitmentHasOverlappingApprenticeships(commitment))
            {
                throw new ValidationException("Unable to approve commitment with overlapping apprenticeships");
            }
        }

        private async Task<Commitment> GetCommitment(long commitmentId)
        {
            var commitment = await _commitmentRepository.GetCommitmentById(commitmentId);
            return commitment;
        }

        private static void CheckCommitmentStatus(Commitment commitment)
        {
            if (commitment.CommitmentStatus == CommitmentStatus.Deleted)
            {
                throw new InvalidOperationException($"Commitment {commitment.Id} cannot be updated because status is {commitment.CommitmentStatus}");
            }
        }

        private static void CheckEditStatus(Commitment commitment)
        {
            if (commitment.EditStatus != EditStatus.Both && commitment.EditStatus != EditStatus.EmployerOnly)
            {
                throw new UnauthorizedException($"Employer not allowed to edit commitment: {commitment.Id}");
            }
        }

        private static void CheckAuthorization(long employerAccountId, Commitment commitment)
        {
            if (commitment.EmployerAccountId != employerAccountId)
            {
                throw new UnauthorizedException($"Employer {employerAccountId} not authorised to access commitment: {commitment.Id}, expected employer {commitment.EmployerAccountId}");
            }
        }

        private static void CheckStateForApproval(Commitment commitment)
        {
            if (!commitment.EmployerCanApproveCommitment)
            {
                throw new InvalidOperationException($"Commitment {commitment.Id} cannot be approved because apprentice information is incomplete");
            }
        }
    }
}
