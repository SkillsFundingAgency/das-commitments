using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Events;
using SFA.DAS.Messaging.Interfaces;

namespace SFA.DAS.Commitments.Application.Commands.UpdateApprenticeship
{
    //todo: add test for UpdateApprenticeshipCommandHandler various scenarios

    public sealed class UpdateApprenticeshipCommandHandler : AsyncRequestHandler<UpdateApprenticeshipCommand>
    {
        private readonly ICommitmentRepository _commitmentRepository;

        private readonly IApprenticeshipRepository _apprenticeshipRepository;

        private readonly AbstractValidator<UpdateApprenticeshipCommand> _validator;
        private readonly IApprenticeshipUpdateRules _apprenticeshipUpdateRules;
        private readonly IApprenticeshipEvents _apprenticeshipEvents;
        private readonly ICommitmentsLogger _logger;
        private readonly IHistoryRepository _historyRepository;
        private HistoryService _historyService;
        private IMessagePublisher _messagePublisher;

        public UpdateApprenticeshipCommandHandler(ICommitmentRepository commitmentRepository,
            IApprenticeshipRepository apprenticeshipRepository,
            AbstractValidator<UpdateApprenticeshipCommand> validator,
            IApprenticeshipUpdateRules apprenticeshipUpdateRules, IApprenticeshipEvents apprenticeshipEvents,
            ICommitmentsLogger logger, IHistoryRepository historyRepository, IMessagePublisher messagePublisher)
        {
            _commitmentRepository = commitmentRepository;
            _apprenticeshipRepository = apprenticeshipRepository;
            _validator = validator;
            _apprenticeshipUpdateRules = apprenticeshipUpdateRules;
            _apprenticeshipEvents = apprenticeshipEvents;
            _logger = logger;
            _historyRepository = historyRepository;
            _messagePublisher = messagePublisher;
        }

        protected override async Task HandleCore(UpdateApprenticeshipCommand command)
        {
            LogMessage(command);

            var validationResult = _validator.Validate(command);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var commitment = await _commitmentRepository.GetCommitmentById(command.CommitmentId);
            var apprenticeship = commitment.Apprenticeships.Single(x => x.Id == command.ApprenticeshipId);
            var transferRejected = commitment.TransferApprovalStatus == TransferApprovalStatus.TransferRejected;

            CheckAuthorization(command, commitment);
            CheckCommitmentStatus(command, commitment);
            CheckEditStatus(command, commitment);
            CheckPaymentStatus(apprenticeship);

            StartTrackingHistory(commitment, command.Caller.CallerType, command.UserId, command.UserName);

            var publishMessage = command.Caller.CallerType == CallerType.Employer
                && _apprenticeshipUpdateRules.DetermineWhetherChangeRequiresAgreement(apprenticeship, command.Apprenticeship)
                && commitment.Apprenticeships.All(x => x.AgreementStatus == AgreementStatus.ProviderAgreed);

            UpdateApprenticeshipEntity(apprenticeship, command.Apprenticeship, command);

            var apprenticeshipStatusUpdates = GetApprenticeshipsRequiringStatusUpdates(commitment, apprenticeship);

            await _apprenticeshipRepository.UpdateApprenticeship(apprenticeship, command.Caller);

            await Task.WhenAll(
                ResetCommitmentTransferRejectionIfRequired(commitment),
                UpdateStatusOfApprenticeships(apprenticeshipStatusUpdates, apprenticeship.AgreementStatus),
                PublishApprenticeshipUpdateEvents(commitment, apprenticeship, transferRejected, apprenticeshipStatusUpdates),
                _historyService.Save()
            );

            if (publishMessage)
            {
                await PublishMessage(commitment);
            }
        }

        private async Task ResetCommitmentTransferRejectionIfRequired(Commitment commitment)
        {
            if (commitment.TransferApprovalStatus != TransferApprovalStatus.TransferRejected)
                return;

            commitment.TransferApprovalStatus = null;
            commitment.TransferApprovalActionedOn = null;
            commitment.LastAction = LastAction.AmendAfterRejected;
            await _commitmentRepository.UpdateCommitment(commitment);
        }

        private async Task PublishApprenticeshipUpdateEvents(Commitment commitment, Apprenticeship updatedApprenticeship, bool transferRejected, List<Apprenticeship> statusUpdates)
        {
            var apprenticeshipsPublish = transferRejected
                ? commitment.Apprenticeships //entire cohort
                : statusUpdates.Union(new List<Apprenticeship>{ updatedApprenticeship }); //just those having had status updates

            foreach (var apprenticeship in apprenticeshipsPublish)
            {
                await _apprenticeshipEvents.PublishEvent(commitment, apprenticeship, "APPRENTICESHIP-UPDATED");
            }          
        }

        private async Task PublishMessage(Commitment commitment)
        {
            await _messagePublisher.PublishAsync(
                      new ProviderCohortApprovalUndoneByEmployerUpdate(
                          commitment.EmployerAccountId,
                          commitment.ProviderId.Value,
                          commitment.Id));
        }

        private async Task UpdateStatusOfApprenticeships(List<Apprenticeship> apprenticeships, AgreementStatus agreementStatus)
        {
            foreach (var apprenticeship in apprenticeships)
            {
                await _apprenticeshipRepository.UpdateApprenticeshipStatus(apprenticeship.CommitmentId, apprenticeship.Id, agreementStatus);
            }
        }

        private List<Apprenticeship> GetApprenticeshipsRequiringStatusUpdates(Commitment commitment, Apprenticeship updatedApprenticeship)
        {
            var result = new List<Apprenticeship>();
        
            foreach (var apprenticeship in commitment.Apprenticeships.Where(x => x.Id != updatedApprenticeship.Id))
            {
                if (apprenticeship.AgreementStatus != updatedApprenticeship.AgreementStatus)
                {
                    apprenticeship.AgreementStatus = updatedApprenticeship.AgreementStatus;
                    result.Add(apprenticeship);
                }
            }

            return result;
        }

        private void StartTrackingHistory(Commitment commitment, CallerType callerType, string userId, string userName)
        {
            _historyService = new HistoryService(_historyRepository);
            _historyService.TrackUpdate(commitment, CommitmentChangeType.EditedApprenticeship.ToString(), commitment.Id, null, callerType, userId, commitment.ProviderId, commitment.EmployerAccountId, userName);

            foreach (var apprenticeship in commitment.Apprenticeships)
            {
                _historyService.TrackUpdate(apprenticeship, ApprenticeshipChangeType.Updated.ToString(), null, apprenticeship.Id, callerType, userId, apprenticeship.ProviderId, apprenticeship.EmployerAccountId, userName);
            }
        }

        private void LogMessage(UpdateApprenticeshipCommand command)
        {
            string messageTemplate = $"{command.Caller.CallerType}: {command.Caller.Id} has called UpdateApprenticeshipCommand";

            if (command.Caller.CallerType == CallerType.Employer)
                _logger.Info(messageTemplate, accountId: command.Caller.Id, apprenticeshipId: command.ApprenticeshipId);
            else
                _logger.Info(messageTemplate, providerId: command.Caller.Id, apprenticeshipId: command.ApprenticeshipId);
        }

        private static void CheckCommitmentStatus(UpdateApprenticeshipCommand message, Commitment commitment)
        {
            if (commitment.CommitmentStatus != CommitmentStatus.New && commitment.CommitmentStatus != CommitmentStatus.Active)
                throw new InvalidOperationException($"Apprenticeship {message.ApprenticeshipId} in commitment {commitment.Id} cannot be updated because status is {commitment.CommitmentStatus}");
        }

        private static void CheckEditStatus(UpdateApprenticeshipCommand message, Commitment commitment)
        {
            switch (message.Caller.CallerType)
            {
                case CallerType.Provider:
                    if (commitment.EditStatus != EditStatus.Both && commitment.EditStatus != EditStatus.ProviderOnly)
                        throw new UnauthorizedException($"Provider {message.Caller.Id} not allowed to edit apprenticeship {message.ApprenticeshipId} in commitment {message.CommitmentId}");
                    break;
                case CallerType.Employer:
                    if (commitment.EditStatus != EditStatus.Both && commitment.EditStatus != EditStatus.EmployerOnly)
                        throw new UnauthorizedException($"Employer {message.Caller.Id} not allowed to edit apprenticeship {message.ApprenticeshipId} in commitment {message.CommitmentId}");
                    break;
            }
        }

        private static void CheckPaymentStatus(Apprenticeship apprenticeship)
        {
            var allowedPaymentStatusesForUpdating = new[] {PaymentStatus.Active, PaymentStatus.PendingApproval, PaymentStatus.Paused};

            if (!allowedPaymentStatusesForUpdating.Contains(apprenticeship.PaymentStatus))
                throw new UnauthorizedException($"Apprenticeship {apprenticeship.Id} cannot be updated when payment status is {apprenticeship.PaymentStatus}");
        }

        private static void CheckAuthorization(UpdateApprenticeshipCommand message, Commitment commitment)
        {
            switch (message.Caller.CallerType)
            {
                case CallerType.Provider:
                    if (commitment.ProviderId != message.Caller.Id)
                        throw new UnauthorizedException($"Provider {message.Caller.Id} not authorised to access commitment: {message.CommitmentId}, expected provider {commitment.ProviderId}");
                    break;
                case CallerType.Employer:
                default:
                    if (commitment.EmployerAccountId != message.Caller.Id)
                        throw new UnauthorizedException($"Employer {message.Caller.Id} not authorised to access commitment: {message.CommitmentId}, expected employer {commitment.EmployerAccountId}");
                    break;
            }
        }

        private void UpdateApprenticeshipEntity(Apprenticeship existingApprenticeship, Apprenticeship updatedApprenticeship, UpdateApprenticeshipCommand message)
        {
            var doChangesRequireAgreement = _apprenticeshipUpdateRules.DetermineWhetherChangeRequiresAgreement(existingApprenticeship, updatedApprenticeship);

            existingApprenticeship.FirstName = updatedApprenticeship.FirstName;
            existingApprenticeship.LastName = updatedApprenticeship.LastName;
            existingApprenticeship.DateOfBirth = updatedApprenticeship.DateOfBirth;
            existingApprenticeship.NINumber = updatedApprenticeship.NINumber;
            existingApprenticeship.ULN = updatedApprenticeship.ULN;
            existingApprenticeship.CommitmentId = message.CommitmentId;
            existingApprenticeship.TrainingType = updatedApprenticeship.TrainingType;
            existingApprenticeship.TrainingCode = updatedApprenticeship.TrainingCode;
            existingApprenticeship.TrainingName = updatedApprenticeship.TrainingName;
            existingApprenticeship.Cost = updatedApprenticeship.Cost;
            existingApprenticeship.StartDate = updatedApprenticeship.StartDate;
            existingApprenticeship.EndDate = updatedApprenticeship.EndDate;
            existingApprenticeship.EmployerRef = updatedApprenticeship.EmployerRef;
            existingApprenticeship.ProviderRef = updatedApprenticeship.ProviderRef;

            existingApprenticeship.AgreementStatus = _apprenticeshipUpdateRules.DetermineNewAgreementStatus(existingApprenticeship.AgreementStatus, message.Caller.CallerType, doChangesRequireAgreement);
            existingApprenticeship.PaymentStatus = _apprenticeshipUpdateRules.DetermineNewPaymentStatus(existingApprenticeship.PaymentStatus, doChangesRequireAgreement);

            

        }
    }
}
