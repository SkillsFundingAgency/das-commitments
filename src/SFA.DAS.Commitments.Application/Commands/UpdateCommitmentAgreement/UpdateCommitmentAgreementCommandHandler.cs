using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;

using SFA.DAS.Commitments.Api.Types.Validation;
using SFA.DAS.Commitments.Application.Commands.SetPaymentOrder;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Application.Queries.GetOverlappingApprenticeships;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;

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
        private readonly IApprenticeshipRepository _apprenticeshipRepository;

        private readonly ICommitmentsLogger _logger;
        private readonly IMediator _mediator;
        private readonly AbstractValidator<UpdateCommitmentAgreementCommand> _validator;

        public UpdateCommitmentAgreementCommandHandler(
            ICommitmentRepository commitmentRepository,
            IApprenticeshipRepository apprenticeshipRepository,
            IApprenticeshipUpdateRules apprenticeshipUpdateRules,
            ICommitmentsLogger logger, 
            IMediator mediator,
            AbstractValidator<UpdateCommitmentAgreementCommand> validator, 
            IApprenticeshipEventsList apprenticeshipEventsList, 
            IApprenticeshipEventsPublisher apprenticeshipEventsPublisher)
        {
            if (commitmentRepository == null)
                throw new ArgumentNullException(nameof(commitmentRepository));
            if (apprenticeshipRepository == null)
                throw new ArgumentNullException(nameof(apprenticeshipRepository));
            if (apprenticeshipUpdateRules == null)
                throw new ArgumentNullException(nameof(apprenticeshipUpdateRules));
            if (apprenticeshipEventsList == null)
                throw new ArgumentNullException(nameof(apprenticeshipEventsList));
            if (apprenticeshipEventsPublisher == null)
                throw new ArgumentNullException(nameof(apprenticeshipEventsPublisher));
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _commitmentRepository = commitmentRepository;
            _apprenticeshipRepository = apprenticeshipRepository;
            _apprenticeshipUpdateRules = apprenticeshipUpdateRules;
            _apprenticeshipEventsList = apprenticeshipEventsList;
            _apprenticeshipEventsPublisher = apprenticeshipEventsPublisher;
            _logger = logger;
            _mediator = mediator;
            _validator = validator;
            _apprenticeshipEventsList = apprenticeshipEventsList;
            _apprenticeshipEventsPublisher = apprenticeshipEventsPublisher;
        }

        protected override async Task HandleCore(UpdateCommitmentAgreementCommand command)
        {
            _validator.ValidateAndThrow(command);

            LogMessage(command);

            var commitment = await _commitmentRepository.GetCommitmentById(command.CommitmentId);

            CheckCommitmentStatus(commitment);
            CheckEditStatus(command, commitment);
            CheckAuthorization(command, commitment);

            if (command.LatestAction == Api.Types.Commitment.Types.LastAction.Approve)
            {
                CheckStateForApproval(commitment, command.Caller);

                var overlaps = await GetOverlappingApprenticeships(commitment);
                if (overlaps.Data.Any())
                {
                    throw new ValidationException("Unable to approve commitment with overlapping apprenticeships");
                }
            }

            var latestAction = (LastAction) command.LatestAction;

            await UpdateApprenticeshipAgreementStatuses(command, commitment, latestAction);

            var updatedCommitment = await _commitmentRepository.GetCommitmentById(command.CommitmentId);
            var areAnyApprenticeshipsPendingAgreement = updatedCommitment.Apprenticeships.Any(a => a.AgreementStatus != AgreementStatus.BothAgreed);

            await UpdateCommitmentStatuses(command, updatedCommitment, areAnyApprenticeshipsPendingAgreement, latestAction);

            // recalculate payment order for all the employer account's apprenticeships if necessary
            await SetPaymentOrderIfNeeded(command.Caller, commitment.EmployerAccountId, commitment.Apprenticeships.Count, latestAction, areAnyApprenticeshipsPendingAgreement);

            await CreateMessage(command);
        }

        private async Task CreateMessage(UpdateCommitmentAgreementCommand command)
        {
            var message = new Message
            {
                Author = command.LastUpdatedByName,
                Text = command.Message ?? string.Empty,
                CreatedBy = command.Caller.CallerType
            };

            await _commitmentRepository.SaveMessage(command.CommitmentId, message);
        }

        private async Task UpdateCommitmentStatuses(UpdateCommitmentAgreementCommand command, Commitment updatedCommitment, bool areAnyApprenticeshipsPendingAgreement, LastAction latestAction)
        {
            updatedCommitment.EditStatus = _apprenticeshipUpdateRules.DetermineNewEditStatus(updatedCommitment.EditStatus, command.Caller.CallerType, areAnyApprenticeshipsPendingAgreement,
                updatedCommitment.Apprenticeships.Count, latestAction);
            updatedCommitment.CommitmentStatus = _apprenticeshipUpdateRules.DetermineNewCommmitmentStatus(areAnyApprenticeshipsPendingAgreement);
            updatedCommitment.LastAction = latestAction;

            SetLastUpdatedDetails(command, updatedCommitment);
            
            await _commitmentRepository.UpdateCommitment(updatedCommitment, command.Caller.CallerType, command.UserId);
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

        private async Task UpdateApprenticeshipAgreementStatuses(UpdateCommitmentAgreementCommand command, Commitment commitment, LastAction latestAction)
        {
            var updatedApprenticeships = new List<Apprenticeship>();
            foreach (var apprenticeship in commitment.Apprenticeships)
            {
                //todo: extract status stuff outside loop and set all apprenticeships to same agreement status?
                var hasChanged = UpdateApprenticeshipStatuses(command, latestAction, apprenticeship);

                if (hasChanged)
                {
                    updatedApprenticeships.Add(apprenticeship);
                    await AddApprenticeshipUpdatedEvent(commitment, apprenticeship);
                }
            }

            await _apprenticeshipRepository.UpdateApprenticeshipStatuses(updatedApprenticeships);
            await _apprenticeshipEventsPublisher.Publish(_apprenticeshipEventsList);
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
                    apprenticeship.AgreedOn = DateTime.Now;
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

        private async Task AddApprenticeshipUpdatedEvent(Commitment commitment, Apprenticeship apprenticeship)
        {
            var effectiveFromDate = await DetermineEffectiveFromDate(apprenticeship.AgreementStatus, apprenticeship.ULN, apprenticeship.StartDate);
            _apprenticeshipEventsList.Add(commitment, apprenticeship, "APPRENTICESHIP-AGREEMENT-UPDATED", effectiveFromDate);
        }

        private async Task<DateTime?> DetermineEffectiveFromDate(AgreementStatus agreementStatus, string uln, DateTime? startDate)
        {
            if (agreementStatus != AgreementStatus.BothAgreed)
            {
                return null;
            }

            var previousApprenticeshipStoppedDate = await GetPreviousApprenticeshipStoppedDate(uln, startDate);
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

        private async Task<DateTime?> GetPreviousApprenticeshipStoppedDate(string uln, DateTime? startDate)
        {
            var previousApprenticeships = await GetPreviousApprenticeships(uln, startDate.Value);
            if (!previousApprenticeships.Any())
            {
                return null;
            }

            var latestApprenticeship = previousApprenticeships.OrderByDescending(x => x.StartDate).First();
            return latestApprenticeship.StopDate;
        }

        private async Task<IEnumerable<ApprenticeshipResult>> GetPreviousApprenticeships(string uln, DateTime startDate)
        {
            var apprenticeships = await _apprenticeshipRepository.GetActiveApprenticeshipsByUlns(new[] { uln });
            return apprenticeships.Where(x => x.StartDate < startDate);
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
                        throw new UnauthorizedException($"Provider {message.Caller.Id} unauthorized to edit commitment: {message.CommitmentId}");
                    break;
                case CallerType.Employer:
                    if (commitment.EditStatus != EditStatus.Both && commitment.EditStatus != EditStatus.EmployerOnly)
                        throw new UnauthorizedException($"Employer {message.Caller.Id} unauthorized to edit commitment: {message.CommitmentId}");
                    break;
            }
        }

        private static void CheckAuthorization(UpdateCommitmentAgreementCommand message, Commitment commitment)
        {
            switch (message.Caller.CallerType)
            {
                case CallerType.Provider:
                    if (commitment.ProviderId != message.Caller.Id)
                        throw new UnauthorizedException($"Provider {message.Caller.Id} unauthorized to view commitment: {message.CommitmentId}");
                    break;
                case CallerType.Employer:
                    if (commitment.EmployerAccountId != message.Caller.Id)
                        throw new UnauthorizedException($"Employer {message.Caller.Id} unauthorized to view commitment: {message.CommitmentId}");
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

        private async Task SetPaymentOrderIfNeeded(Caller caller, long employerAccountId, int apprenticeshipsCount, LastAction latestAction, bool areAnyApprenticeshipsPendingAgreement)
        {
            if (latestAction == LastAction.Approve && apprenticeshipsCount > 0 && !areAnyApprenticeshipsPendingAgreement)
            {
                await _mediator.SendAsync(new SetPaymentOrderCommand {AccountId = employerAccountId});
            }
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
