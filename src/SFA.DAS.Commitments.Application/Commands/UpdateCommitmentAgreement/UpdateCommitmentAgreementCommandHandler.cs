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
        private readonly IMessagePublisher _messagePublisher;

        private readonly ICommitmentsLogger _logger;
        private readonly AbstractValidator<UpdateCommitmentAgreementCommand> _validator;

        public UpdateCommitmentAgreementCommandHandler(ICommitmentRepository commitmentRepository, IApprenticeshipRepository apprenticeshipRepository, IApprenticeshipUpdateRules apprenticeshipUpdateRules, ICommitmentsLogger logger, AbstractValidator<UpdateCommitmentAgreementCommand> validator, IApprenticeshipEventsList apprenticeshipEventsList, IApprenticeshipEventsPublisher apprenticeshipEventsPublisher, IHistoryRepository historyRepository, IMessagePublisher messagePublisher)
        {
            _commitmentRepository = commitmentRepository;
            _apprenticeshipRepository = apprenticeshipRepository;
            _apprenticeshipUpdateRules = apprenticeshipUpdateRules;
            _apprenticeshipEventsList = apprenticeshipEventsList;
            _apprenticeshipEventsPublisher = apprenticeshipEventsPublisher;
            _historyRepository = historyRepository;
            _messagePublisher = messagePublisher;
            _logger = logger;
            _validator = validator;
        }

        protected override async Task HandleCore(UpdateCommitmentAgreementCommand command)
        {
            _validator.ValidateAndThrow(command);

            LogMessage(command);

            var commitment = await _commitmentRepository.GetCommitmentById(command.CommitmentId);

            CheckCommitmentCanBeUpdated(command, commitment);
            
            var providerHasPreviouslyApprovedCommitment = commitment.Apprenticeships.All(a => a.AgreementStatus == AgreementStatus.ProviderAgreed);

            var updatedApprenticeships = await UpdateApprenticeshipAgreementStatuses(command, commitment, command.LatestAction);

            var anyApprenticeshipsPendingAgreement = commitment.Apprenticeships.Any(a => a.AgreementStatus != AgreementStatus.BothAgreed);
            await UpdateCommitmentStatuses(command, commitment, anyApprenticeshipsPendingAgreement, command.LatestAction);
            await CreateCommitmentMessage(command, commitment);

            await PublishEventsForUpdatedApprenticeships(commitment, updatedApprenticeships);

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

        private static bool ApprovedCommitmentIsBeingReturnedToProvider(UpdateCommitmentAgreementCommand command, bool providerHasPreviouslyApprovedCommitment)
        {
            return providerHasPreviouslyApprovedCommitment && command.LatestAction == LastAction.Amend && command.Caller.CallerType == CallerType.Employer;
        }

        private async Task PublishEventsForUpdatedApprenticeships(Commitment commitment, IList<Apprenticeship> updatedApprenticeships)
        {
            await CreateEventsForUpdatedApprenticeships(commitment, updatedApprenticeships);
            await _apprenticeshipEventsPublisher.Publish(_apprenticeshipEventsList);
        }

        private async Task CreateCommitmentMessage(UpdateCommitmentAgreementCommand command, Commitment commitment)
        {
            var cohortStatusChangeService = new CohortStatusChangeService(_commitmentRepository);
            await cohortStatusChangeService.AddMessageToCommitment(commitment, command.LastUpdatedByName, command.Message, command.Caller.CallerType);
        }

        private async Task UpdateCommitmentStatuses(UpdateCommitmentAgreementCommand command, Commitment updatedCommitment, bool areAnyApprenticeshipsPendingAgreement, LastAction latestAction)
        {
            var updatedEditStatus = _apprenticeshipUpdateRules.DetermineNewEditStatus(updatedCommitment.EditStatus, command.Caller.CallerType, areAnyApprenticeshipsPendingAgreement,
                updatedCommitment.Apprenticeships.Count, latestAction);
            var changeType = DetermineHistoryChangeType();
            var historyService = new HistoryService(_historyRepository);
            historyService.TrackUpdate(updatedCommitment, changeType.ToString(), updatedCommitment.Id, null, command.Caller.CallerType, command.UserId, updatedCommitment.ProviderId, updatedCommitment.EmployerAccountId, command.LastUpdatedByName);

            updatedCommitment.EditStatus = updatedEditStatus;
            updatedCommitment.CommitmentStatus = _apprenticeshipUpdateRules.DetermineNewCommmitmentStatus(areAnyApprenticeshipsPendingAgreement);
            updatedCommitment.LastAction = latestAction;
            updatedCommitment.TransferApprovalStatus = TransferApprovalStatus.Pending;

            SetLastUpdatedDetails(command, updatedCommitment);
            await _commitmentRepository.UpdateCommitment(updatedCommitment);
            await historyService.Save();
        }

        private CommitmentChangeType DetermineHistoryChangeType()
        {
            var changeType = CommitmentChangeType.SentForReview;
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
            Parallel.ForEach(updatedApprenticeships, apprenticeship =>
            {
                AddApprenticeshipUpdatedEvent(commitment, apprenticeship);
            });
        }

        private bool UpdateApprenticeshipStatuses(UpdateCommitmentAgreementCommand command, LastAction latestAction, Apprenticeship apprenticeship)
        {
            bool hasChanged = false;

            var newApprenticeshipAgreementStatus = _apprenticeshipUpdateRules.DetermineNewAgreementStatus(apprenticeship.AgreementStatus, command.Caller.CallerType, latestAction);
            var newApprenticeshipPaymentStatus = PaymentStatus.PendingApproval;

            if (apprenticeship.AgreementStatus != newApprenticeshipAgreementStatus)
            {
                apprenticeship.AgreementStatus = newApprenticeshipAgreementStatus;
                hasChanged = true;
            }

            if (apprenticeship.PaymentStatus != newApprenticeshipPaymentStatus)
            {
                apprenticeship.PaymentStatus = newApprenticeshipPaymentStatus;
                hasChanged = true;
            }
            return hasChanged;
        }

        private void AddApprenticeshipUpdatedEvent(Commitment commitment, Apprenticeship apprenticeship)
        {
            _apprenticeshipEventsList.Add(commitment, apprenticeship, "APPRENTICESHIP-AGREEMENT-UPDATED");
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
    }
}
