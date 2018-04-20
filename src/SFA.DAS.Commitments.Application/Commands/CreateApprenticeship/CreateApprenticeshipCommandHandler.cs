using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Events;
using SFA.DAS.Messaging.Interfaces;
using Commitment = SFA.DAS.Commitments.Domain.Entities.Commitment;

namespace SFA.DAS.Commitments.Application.Commands.CreateApprenticeship
{
    public sealed class CreateApprenticeshipCommandHandler : IAsyncRequestHandler<CreateApprenticeshipCommand, long>
    {
        private readonly ICommitmentRepository _commitmentRepository;

        private readonly IApprenticeshipRepository _apprenticeshipRepository;

        private readonly AbstractValidator<CreateApprenticeshipCommand> _validator;
        private readonly IApprenticeshipEvents _apprenticeshipEvents;
        private readonly ICommitmentsLogger _logger;
        private readonly IHistoryRepository _historyRepository;
        private HistoryService _historyService;
        private IMessagePublisher _messagePublisher;

        public CreateApprenticeshipCommandHandler(ICommitmentRepository commitmentRepository, IApprenticeshipRepository apprenticeshipRepository, AbstractValidator<CreateApprenticeshipCommand> validator, IApprenticeshipEvents apprenticeshipEvents, ICommitmentsLogger logger, IHistoryRepository historyRepository, IMessagePublisher messagePublisher)
        {
            _commitmentRepository = commitmentRepository;
            _apprenticeshipRepository = apprenticeshipRepository;
            _validator = validator;
            _apprenticeshipEvents = apprenticeshipEvents;
            _logger = logger;
            _historyRepository = historyRepository;
            _messagePublisher = messagePublisher;
        }

        public async Task<long> Handle(CreateApprenticeshipCommand command)
        {
            LogMessage(command);

            var validationResult = _validator.Validate(command);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            // TODO: Throw Exception if commitment doesn't exist
            var commitment = await _commitmentRepository.GetCommitmentById(command.CommitmentId);
            var transferRejected = commitment.TransferApprovalStatus == TransferApprovalStatus.TransferRejected;

            CheckAuthorization(command, commitment);
            CheckEditStatus(command, commitment);
            CheckCommitmentStatus(commitment);

            var publishMessage = command.Caller.CallerType == CallerType.Employer
                && commitment.Apprenticeships.Any()
                && commitment.Apprenticeships.All(x => x.AgreementStatus == AgreementStatus.ProviderAgreed);

            StartTrackingHistory(commitment, command.Caller.CallerType, command.UserId, command.UserName);

            var apprenticeship = UpdateApprenticeship(command.Apprenticeship, command);
            var apprenticeshipId = await _apprenticeshipRepository.CreateApprenticeship(apprenticeship);
            var savedApprenticeship = await _apprenticeshipRepository.GetApprenticeship(apprenticeshipId);

            _historyService.TrackInsert(savedApprenticeship, ApprenticeshipChangeType.Created.ToString(), null,
                savedApprenticeship.Id, command.Caller.CallerType, command.UserId, savedApprenticeship.ProviderId,
                savedApprenticeship.EmployerAccountId, command.UserName);

            await ResetCommitmentTransferRejection(commitment);

            var apprenticeshipStatusUpdates = GetApprenticeshipsRequiringStatusUpdates(commitment, apprenticeship);

            await Task.WhenAll(
                
                _apprenticeshipEvents.PublishEvent(commitment, savedApprenticeship, "APPRENTICESHIP-CREATED"),
                PublishApprenticeshipUpdateEvents(commitment, transferRejected, apprenticeshipStatusUpdates),
                UpdateStatusOfApprenticeships(apprenticeshipStatusUpdates, AgreementStatus.NotAgreed),
                _historyService.Save()
            );

            if (publishMessage)
            {
                await PublishMessage(commitment);
            }

            return apprenticeshipId;
        }

        private void StartTrackingHistory(Commitment commitment, CallerType callerType, string userId, string userName)
        {
            _historyService = new HistoryService(_historyRepository);
            _historyService.TrackUpdate(commitment, CommitmentChangeType.CreatedApprenticeship.ToString(), commitment.Id, null, callerType, userId, commitment.ProviderId, commitment.EmployerAccountId, userName);

            foreach (var apprenticeship in commitment.Apprenticeships)
            {
                _historyService.TrackUpdate(apprenticeship, ApprenticeshipChangeType.Updated.ToString(), null, apprenticeship.Id, callerType, userId, apprenticeship.ProviderId, apprenticeship.EmployerAccountId, userName);
            }
        }

        private async Task PublishApprenticeshipUpdateEvents(Commitment commitment, bool transferRejected, List<Apprenticeship> statusUpdates)
        {
            var apprenticeshipsPublish = transferRejected
                ? commitment.Apprenticeships //entire cohort
                : statusUpdates; //just those having had status updates

            foreach (var apprenticeship in apprenticeshipsPublish)
            {
                await _apprenticeshipEvents.PublishEvent(commitment, apprenticeship, "APPRENTICESHIP-UPDATED");
            }
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

        private async Task ResetCommitmentTransferRejection(Commitment commitment)
        {
            if (commitment.TransferApprovalStatus != TransferApprovalStatus.TransferRejected)
                return;

            commitment.TransferApprovalStatus = null;
            commitment.TransferApprovalActionedOn = null;
            commitment.LastAction = LastAction.AmendAfterRejected;
            await _commitmentRepository.UpdateCommitment(commitment);
        }


        private async Task PublishMessage(Commitment commitment)
        {
            await _messagePublisher.PublishAsync(
                      new ProviderCohortApprovalUndoneByEmployerUpdate(
                          commitment.EmployerAccountId,
                          commitment.ProviderId.Value,
                          commitment.Id));
        }

        private Apprenticeship UpdateApprenticeship(Apprenticeship apprenticeship, CreateApprenticeshipCommand command)
        {
            apprenticeship.CommitmentId = command.CommitmentId;
            apprenticeship.PaymentStatus = PaymentStatus.PendingApproval;
            return apprenticeship;
        }

        private static void CheckAuthorization(CreateApprenticeshipCommand message, Commitment commitment)
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

        private static void CheckCommitmentStatus(Commitment commitment)
        {
            if (commitment.CommitmentStatus != Domain.Entities.CommitmentStatus.New && commitment.CommitmentStatus != Domain.Entities.CommitmentStatus.Active)
                throw new InvalidOperationException($"Cannot add apprenticeship in commitment {commitment.Id} because status is {commitment.CommitmentStatus}");
        }

        private static void CheckEditStatus(CreateApprenticeshipCommand message, Commitment commitment)
        {
            switch (message.Caller.CallerType)
            {
                case CallerType.Provider:
                    if (commitment.EditStatus != Domain.Entities.EditStatus.Both && commitment.EditStatus != Domain.Entities.EditStatus.ProviderOnly)
                        throw new UnauthorizedException($"Provider {message.Caller.Id} not allowed to add apprenticeship {message.Apprenticeship.Id} to commitment {message.CommitmentId}");
                    break;
                case CallerType.Employer:
                    if (commitment.EditStatus != Domain.Entities.EditStatus.Both && commitment.EditStatus != Domain.Entities.EditStatus.EmployerOnly)
                        throw new UnauthorizedException($"Employer {message.Caller.Id} not allowed to add apprenticeship {message.Apprenticeship.Id} to commitment {message.CommitmentId}");
                    break;
            }
        }

        private void LogMessage(CreateApprenticeshipCommand command)
        {
            string messageTemplate = $"{command.Caller.CallerType}: {command.Caller.Id} has called CreateApprenticeshipCommand";

            if (command.Caller.CallerType == CallerType.Employer)
                _logger.Info(messageTemplate, accountId: command.Caller.Id, commitmentId: command.CommitmentId);
            else
                _logger.Info(messageTemplate, providerId: command.Caller.Id, commitmentId: command.CommitmentId);
        }
    }
}
