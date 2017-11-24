using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;

using SFA.DAS.Commitments.Application.Commands.SetPaymentOrder;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Application.Queries.GetOverlappingApprenticeships;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Events;
using SFA.DAS.Messaging.Interfaces;
using AgreementStatus = SFA.DAS.Commitments.Domain.Entities.AgreementStatus;
using Commitment = SFA.DAS.Commitments.Domain.Entities.Commitment;
using CommitmentStatus = SFA.DAS.Commitments.Domain.Entities.CommitmentStatus;
using EditStatus = SFA.DAS.Commitments.Domain.Entities.EditStatus;
using LastAction = SFA.DAS.Commitments.Domain.Entities.LastAction;

namespace SFA.DAS.Commitments.Application.Commands.UpdateCommitmentAgreement
{
    public sealed class UpdateCommitmentAgreementCommandHandler : AsyncRequestHandler<UpdateCommitmentAgreementCommand>
    {
        private readonly IApprenticeshipUpdateRules _apprenticeshipUpdateRules;
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly IApprenticeshipEventsList _apprenticeshipEventsList;
        private readonly IApprenticeshipEventsPublisher _apprenticeshipEventsPublisher;
        private readonly IHistoryRepository _historyRepository;
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly ICurrentDateTime _currentDateTime;
        private readonly IMessagePublisher _messagePublisher;

        private readonly ICommitmentsLogger _logger;
        private readonly IMediator _mediator;
        private readonly AbstractValidator<UpdateCommitmentAgreementCommand> _validator;

        public UpdateCommitmentAgreementCommandHandler(ICommitmentRepository commitmentRepository, IApprenticeshipRepository apprenticeshipRepository, IApprenticeshipUpdateRules apprenticeshipUpdateRules, ICommitmentsLogger logger, IMediator mediator, AbstractValidator<UpdateCommitmentAgreementCommand> validator, IApprenticeshipEventsList apprenticeshipEventsList, IApprenticeshipEventsPublisher apprenticeshipEventsPublisher, IHistoryRepository historyRepository, ICurrentDateTime currentDateTime, IMessagePublisher messagePublisher)
        {
            _commitmentRepository = commitmentRepository;
            _apprenticeshipRepository = apprenticeshipRepository;
            _apprenticeshipUpdateRules = apprenticeshipUpdateRules;
            _apprenticeshipEventsList = apprenticeshipEventsList;
            _apprenticeshipEventsPublisher = apprenticeshipEventsPublisher;
            _historyRepository = historyRepository;
            _currentDateTime = currentDateTime;
            _messagePublisher = messagePublisher;
            _logger = logger;
            _mediator = mediator;
            _validator = validator;
        }

        protected override async Task HandleCore(UpdateCommitmentAgreementCommand command)
        {
            _validator.ValidateAndThrow(command);

            LogMessage(command);

            var commitment = await _commitmentRepository.GetCommitmentById(command.CommitmentId);

            CheckCommitmentCanBeUpdated(command, commitment);

            if(command.LatestAction == LastAction.Approve)
            {
                await CheckCommitmentCanBeApproved(command, commitment);
            }

            var providerHasPreviouslyApprovedCommitment = commitment.Apprenticeships.All(a => a.AgreementStatus == AgreementStatus.ProviderAgreed);

            var updatedApprenticeships = await UpdateApprenticeshipAgreementStatuses(command, commitment, command.LatestAction);

            var anyApprenticeshipsPendingAgreement = commitment.Apprenticeships.Any(a => a.AgreementStatus != AgreementStatus.BothAgreed);
            await UpdateCommitmentStatuses(command, commitment, anyApprenticeshipsPendingAgreement, command.LatestAction);
            await CreateCommitmentMessage(command, commitment);

            if (IsFinalApproval(command.LatestAction, commitment, anyApprenticeshipsPendingAgreement))
            {
                await CreatePriceHistory(commitment, updatedApprenticeships);
            }

            await PublishEventsForUpdatedApprenticeships(commitment, updatedApprenticeships);

            if (command.LatestAction == LastAction.Approve && commitment.Apprenticeships.Count > 0)
            {
                if (!anyApprenticeshipsPendingAgreement)
                {
                    await _mediator.SendAsync(new SetPaymentOrderCommand {AccountId = commitment.EmployerAccountId});
                }
                await PublishApprovalEvent(commitment, anyApprenticeshipsPendingAgreement, command.Caller.CallerType);
            }

            if (ApprovedCommitmentIsBeingReturnedToProvider(command, providerHasPreviouslyApprovedCommitment))
            {
                await _messagePublisher.PublishAsync(new ApprovedCohortReturnedToProvider(commitment.EmployerAccountId, commitment.ProviderId.Value, commitment.Id));
            }
        }

        private static void CheckCommitmentCanBeUpdated(UpdateCommitmentAgreementCommand command, Commitment commitment)
        {
            CheckCommitmentStatus(commitment);
            CheckEditStatus(command, commitment);
            CheckAuthorization(command, commitment);
        }

        private async Task CheckCommitmentCanBeApproved(UpdateCommitmentAgreementCommand command, Commitment commitment)
        {
            CheckStateForApproval(commitment, command.Caller);
            var overlaps = await GetOverlappingApprenticeships(commitment);
            if (overlaps.Data.Any())
            {
                throw new ValidationException("Unable to approve commitment with overlapping apprenticeships");
            }
        }

        private static bool ApprovedCommitmentIsBeingReturnedToProvider(UpdateCommitmentAgreementCommand command, bool providerHasPreviouslyApprovedCommitment)
        {
            return providerHasPreviouslyApprovedCommitment && command.LatestAction == LastAction.Amend && command.Caller.CallerType == CallerType.Employer;
        }

        private async Task PublishEventsForUpdatedApprenticeships(Commitment commitment, IList<Apprenticeship> updatedApprenticeships)
        {
            await CreateEventsForUpdatedApprenticeships(commitment, updatedApprenticeships);
            await _apprenticeshipEventsPublisher.Publish(_apprenticeshipEventsList);
        }

        private static bool IsFinalApproval(LastAction latestAction, Commitment commitment, bool anyApprenticeshipsPendingAgreement)
        {
            return latestAction == LastAction.Approve && commitment.Apprenticeships.Count > 0 && !anyApprenticeshipsPendingAgreement;
        }

        private async Task CreatePriceHistory(Commitment commitment, IList<Apprenticeship> updatedApprenticeships)
        {
            await _apprenticeshipRepository.CreatePriceHistoryForApprenticeshipsInCommitment(commitment.Id);

            //create price history for purposes of event creation
            foreach (var updatedApprenticeship in updatedApprenticeships)
            {
                updatedApprenticeship.PriceHistory = new List<PriceHistory>
                {
                    new PriceHistory
                    {
                        ApprenticeshipId = updatedApprenticeship.Id,
                        Cost = updatedApprenticeship.Cost.Value,
                        FromDate = updatedApprenticeship.StartDate.Value
                    }
                };
            }
        }

        private async Task PublishApprovalEvent(Commitment commitment, bool anyApprenticeshipsPendingAgreement, CallerType callerType)
        {
            if (!anyApprenticeshipsPendingAgreement && callerType == CallerType.Employer)
            {
                await _messagePublisher.PublishAsync(new CohortApprovedByEmployer(commitment.EmployerAccountId, commitment.ProviderId.Value, commitment.Id));
            } else if (anyApprenticeshipsPendingAgreement && callerType == CallerType.Provider)
            {
                await _messagePublisher.PublishAsync(new CohortApprovalRequestedByProvider(commitment.EmployerAccountId, commitment.ProviderId.Value, commitment.Id));
            }
        }

        private async Task CreateCommitmentMessage(UpdateCommitmentAgreementCommand command, Commitment commitment)
        {
            var message = new Message
            {
                Author = command.LastUpdatedByName,
                Text = command.Message ?? string.Empty,
                CreatedBy = command.Caller.CallerType
            };
            commitment.Messages.Add(message);
            
            await _commitmentRepository.SaveMessage(command.CommitmentId, message);
        }

        private async Task UpdateCommitmentStatuses(UpdateCommitmentAgreementCommand command, Commitment updatedCommitment, bool areAnyApprenticeshipsPendingAgreement, LastAction latestAction)
        {
            var updatedEditStatus = _apprenticeshipUpdateRules.DetermineNewEditStatus(updatedCommitment.EditStatus, command.Caller.CallerType, areAnyApprenticeshipsPendingAgreement,
                updatedCommitment.Apprenticeships.Count, latestAction);
            var changeType = DetermineHistoryChangeType(latestAction, updatedEditStatus);
            var historyService = new HistoryService(_historyRepository);
            historyService.TrackUpdate(updatedCommitment, changeType.ToString(), updatedCommitment.Id, null, command.Caller.CallerType, command.UserId, updatedCommitment.ProviderId, updatedCommitment.EmployerAccountId, command.LastUpdatedByName);

            updatedCommitment.EditStatus = updatedEditStatus;
            updatedCommitment.CommitmentStatus = _apprenticeshipUpdateRules.DetermineNewCommmitmentStatus(areAnyApprenticeshipsPendingAgreement);
            updatedCommitment.LastAction = latestAction;

            SetLastUpdatedDetails(command, updatedCommitment);
            await _commitmentRepository.UpdateCommitment(updatedCommitment);
            await historyService.Save();
        }

        private CommitmentChangeType DetermineHistoryChangeType(LastAction latestAction, EditStatus updatedEditStatus)
        {
            var changeType = CommitmentChangeType.SentForReview;
            if (updatedEditStatus == EditStatus.Both && latestAction == LastAction.Approve)
                changeType = CommitmentChangeType.FinalApproval;
            else if (latestAction == LastAction.Approve)
                changeType = CommitmentChangeType.SentForApproval;

            return changeType;
        }

        private static void SetLastUpdatedDetails(UpdateCommitmentAgreementCommand command, Commitment updatedCommitment)
        {
            if (command.Caller.CallerType == CallerType.Employer)
            {
                updatedCommitment.LastUpdatedByEmployerEmail = command.LastUpdatedByEmail;
                updatedCommitment.LastUpdatedByEmployerName = command.LastUpdatedByName;
            }
            else
            {
                updatedCommitment.LastUpdatedByProviderEmail = command.LastUpdatedByEmail;
                updatedCommitment.LastUpdatedByProviderName = command.LastUpdatedByName;
            }
        }

        private async Task<IList<Apprenticeship>> UpdateApprenticeshipAgreementStatuses(UpdateCommitmentAgreementCommand command, Commitment commitment, LastAction latestAction)
        {
            var updatedApprenticeships = new List<Apprenticeship>();
            foreach (var apprenticeship in commitment.Apprenticeships)
            {
                //todo: extract status stuff outside loop and set all apprenticeships to same agreement status?
                var hasChanged = UpdateApprenticeshipStatuses(command, latestAction, apprenticeship);

                if (hasChanged)
                {
                    updatedApprenticeships.Add(apprenticeship);
                }
            }

            await _apprenticeshipRepository.UpdateApprenticeshipStatuses(updatedApprenticeships);

            return updatedApprenticeships;
        }

        private async Task CreateEventsForUpdatedApprenticeships(Commitment commitment, IList<Apprenticeship> updatedApprenticeships)
        {
            var existingApprenticeships = await GetActiveApprenticeshipsForLearners(updatedApprenticeships);

            Parallel.ForEach(updatedApprenticeships, apprenticeship =>
            {
                AddApprenticeshipUpdatedEvent(commitment, apprenticeship, existingApprenticeships.Where(x => x.Uln == apprenticeship.ULN));
            });
        }

        private async Task<IEnumerable<ApprenticeshipResult>>  GetActiveApprenticeshipsForLearners(IList<Apprenticeship> updatedApprenticeships)
        {
            var ulns = updatedApprenticeships.Select(x => x.ULN);
            var apprenticeships = await _apprenticeshipRepository.GetActiveApprenticeshipsByUlns(ulns);
            return apprenticeships;
        }

        private bool UpdateApprenticeshipStatuses(UpdateCommitmentAgreementCommand command, LastAction latestAction, Apprenticeship apprenticeship)
        {
            bool hasChanged = false;

            var newApprenticeshipAgreementStatus = _apprenticeshipUpdateRules.DetermineNewAgreementStatus(apprenticeship.AgreementStatus, command.Caller.CallerType, latestAction);
            var newApprenticeshipPaymentStatus = _apprenticeshipUpdateRules.DetermineNewPaymentStatus(apprenticeship.PaymentStatus, newApprenticeshipAgreementStatus);

            if (apprenticeship.AgreementStatus != newApprenticeshipAgreementStatus)
            {
                apprenticeship.AgreementStatus = newApprenticeshipAgreementStatus;
                if (apprenticeship.AgreementStatus == AgreementStatus.BothAgreed && !apprenticeship.AgreedOn.HasValue)
                {
                    apprenticeship.AgreedOn = _currentDateTime.Now;
                }
                hasChanged = true;
            }

            if (apprenticeship.PaymentStatus != newApprenticeshipPaymentStatus)
            {
                apprenticeship.PaymentStatus = newApprenticeshipPaymentStatus;
                hasChanged = true;
            }
            return hasChanged;
        }

        private void AddApprenticeshipUpdatedEvent(Commitment commitment, Apprenticeship apprenticeship, IEnumerable<ApprenticeshipResult> existingApprenticeships)
        {
            var effectiveFromDate = DetermineEffectiveFromDate(apprenticeship.AgreementStatus, existingApprenticeships, apprenticeship.StartDate);
            _apprenticeshipEventsList.Add(commitment, apprenticeship, "APPRENTICESHIP-AGREEMENT-UPDATED", effectiveFromDate);
        }

        private DateTime? DetermineEffectiveFromDate(AgreementStatus agreementStatus, IEnumerable<ApprenticeshipResult> existingApprenticeships, DateTime? startDate)
        {
            if (agreementStatus != AgreementStatus.BothAgreed)
            {
                return null;
            }

            var previousApprenticeshipStoppedDate = GetPreviousApprenticeshipStoppedDate(existingApprenticeships, startDate);
            if (HasPreviousApprenticeshipStoppedInTheSameMonth(previousApprenticeshipStoppedDate, startDate))
            {
                return previousApprenticeshipStoppedDate.Value.AddDays(1);
            }
            
            return new DateTime(startDate.Value.Year, startDate.Value.Month, 1);
        }

        private bool HasPreviousApprenticeshipStoppedInTheSameMonth(DateTime? previousApprenticeshipStoppedDate, DateTime? startDate)
        {
            if (!previousApprenticeshipStoppedDate.HasValue)
            {
                return false;
            }

            if (previousApprenticeshipStoppedDate.Value.Year != startDate.Value.Year || previousApprenticeshipStoppedDate.Value.Month != startDate.Value.Month)
            {
                return false;
            }

            return true;
        }

        private DateTime? GetPreviousApprenticeshipStoppedDate(IEnumerable<ApprenticeshipResult> existingApprenticeships, DateTime? startDate)
        {
            var previousApprenticeships = GetPreviousApprenticeships(existingApprenticeships, startDate.Value);
            if (!previousApprenticeships.Any())
            {
                return null;
            }

            var latestApprenticeship = previousApprenticeships.OrderByDescending(x => x.StartDate).First();
            return latestApprenticeship.StopDate;
        }

        private IEnumerable<ApprenticeshipResult> GetPreviousApprenticeships(IEnumerable<ApprenticeshipResult> existingApprenticeships, DateTime startDate)
        {
            return existingApprenticeships.Where(x => x.StartDate < startDate);
        }

        private void LogMessage(UpdateCommitmentAgreementCommand command)
        {
            string messageTemplate = $"{command.Caller.CallerType}: {command.Caller.Id} has called UpdateCommitmentAgreement for commitment {command.CommitmentId} with agreement status: {command.LatestAction}";

            if (command.Caller.CallerType == CallerType.Employer)
                _logger.Info(messageTemplate, accountId: command.Caller.Id, commitmentId: command.CommitmentId);
            else
                _logger.Info(messageTemplate, providerId: command.Caller.Id, commitmentId: command.CommitmentId);
        }

        private static void CheckCommitmentStatus(Commitment commitment)
        {
            if (commitment.CommitmentStatus != CommitmentStatus.New && commitment.CommitmentStatus != CommitmentStatus.Active)
                throw new InvalidOperationException($"Commitment {commitment.Id} cannot be updated because status is {commitment.CommitmentStatus}");
        }

        private static void CheckEditStatus(UpdateCommitmentAgreementCommand message, Commitment commitment)
        {
            switch (message.Caller.CallerType)
            {
                case CallerType.Provider:
                    if (commitment.EditStatus != EditStatus.Both && commitment.EditStatus != EditStatus.ProviderOnly)
                        throw new UnauthorizedException($"Provider {message.Caller.Id} not allowed to edit commitment: {message.CommitmentId}");
                    break;
                case CallerType.Employer:
                    if (commitment.EditStatus != EditStatus.Both && commitment.EditStatus != EditStatus.EmployerOnly)
                        throw new UnauthorizedException($"Employer {message.Caller.Id} not allowed to edit commitment: {message.CommitmentId}");
                    break;
            }
        }

        private static void CheckAuthorization(UpdateCommitmentAgreementCommand message, Commitment commitment)
        {
            switch (message.Caller.CallerType)
            {
                case CallerType.Provider:
                    if (commitment.ProviderId != message.Caller.Id)
                        throw new UnauthorizedException($"Provider {message.Caller.Id} not authorised to access commitment: {message.CommitmentId}, expected provider {commitment.ProviderId}");
                    break;
                case CallerType.Employer:
                    if (commitment.EmployerAccountId != message.Caller.Id)
                        throw new UnauthorizedException($"Employer {message.Caller.Id} not authorised to access commitment: {message.CommitmentId}, expected employer {commitment.EmployerAccountId}");
                    break;
            }
        }

        private static void CheckStateForApproval(Commitment commitment, Caller caller)
        {
            var canBeApprovedByParty = caller.CallerType == CallerType.Employer
                ? commitment.EmployerCanApproveCommitment
                : commitment.ProviderCanApproveCommitment;

            if (!canBeApprovedByParty)
                throw new InvalidOperationException($"Commitment {commitment.Id} cannot be approved because apprentice information is incomplete");
        }

        private async Task<GetOverlappingApprenticeshipsResponse> GetOverlappingApprenticeships(Commitment commitment)
        {
            var overlapRequests = 
                commitment.Apprenticeships
                .Where(x => !string.IsNullOrWhiteSpace(x.ULN) && x.StartDate.HasValue && x.EndDate.HasValue)
                .Select(apprenticeship => 
                    new ApprenticeshipOverlapValidationRequest
                        {
                            Uln = apprenticeship.ULN,
                            StartDate = apprenticeship.StartDate.Value,
                            EndDate = apprenticeship.EndDate.Value
                        })
                .ToList();

            var response = await _mediator.SendAsync(new GetOverlappingApprenticeshipsRequest
            {
                OverlappingApprenticeshipRequests = overlapRequests
            });

            return response;
        }
    }
}
