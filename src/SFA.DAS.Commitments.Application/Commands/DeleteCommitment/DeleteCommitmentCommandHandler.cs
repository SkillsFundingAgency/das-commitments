using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;

using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Events;
using SFA.DAS.Messaging.Interfaces;

namespace SFA.DAS.Commitments.Application.Commands.DeleteCommitment
{
    public sealed class DeleteCommitmentCommandHandler : AsyncRequestHandler<DeleteCommitmentCommand>
    {
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly AbstractValidator<DeleteCommitmentCommand> _validator;
        private readonly ICommitmentsLogger _logger;
        private readonly IApprenticeshipEvents _apprenticeshipEvents;
        private readonly IHistoryRepository _historyRepository;
        private readonly IMessagePublisher _messagePublisher;
        private readonly IV2EventsPublisher _v2EventsPublisher;

        public DeleteCommitmentCommandHandler(ICommitmentRepository commitmentRepository, AbstractValidator<DeleteCommitmentCommand> validator, ICommitmentsLogger logger, IApprenticeshipEvents apprenticeshipEvents, IHistoryRepository historyRepository, IMessagePublisher messagePublisher, IV2EventsPublisher v2EventsPublisher)
        {
            _commitmentRepository = commitmentRepository;
            _validator = validator;
            _logger = logger;
            _apprenticeshipEvents = apprenticeshipEvents;
            _historyRepository = historyRepository;
            _messagePublisher = messagePublisher;
            _v2EventsPublisher = v2EventsPublisher;
        }

        protected override async Task HandleCore(DeleteCommitmentCommand command)
        {
            var validationResult = _validator.Validate(command);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);
            
            LogMessage(command);

            var commitment = await _commitmentRepository.GetCommitmentById(command.CommitmentId);

            if (commitment == null)
            {
                throw new ResourceNotFoundException();
            }

            CheckAuthorization(command, commitment);
            CheckCommitmentStatus(commitment);
            CheckEditStatus(command, commitment);
            CheckPaymentStatus(commitment.Apprenticeships);

            await _commitmentRepository.DeleteCommitment(command.CommitmentId);
            await CreateHistory(commitment, command.Caller.CallerType, command.UserId, command.UserName);
            await _apprenticeshipEvents.BulkPublishDeletionEvent(commitment, commitment.Apprenticeships, "APPRENTICESHIP-DELETED");
            await PublishMessageIfProviderApprovedCohortDeletedByEmployer(commitment, command.Caller.CallerType);
            await PublishMessageIfChangeOfPartyCohortRejected(commitment, command.Caller.CallerType);
            await PublishV2Events(commitment);
        }

        private async Task PublishMessageIfProviderApprovedCohortDeletedByEmployer(Commitment commitment, CallerType callerType)
        {
            // called by employer and provider has previously approved commitment
            if (callerType == CallerType.Employer && commitment.Apprenticeships.All(a => a.AgreementStatus == AgreementStatus.ProviderAgreed))
            {
                await _messagePublisher.PublishAsync(
                    new ProviderCohortApprovalUndoneByEmployerUpdate(commitment.EmployerAccountId, commitment.ProviderId.Value, commitment.Id));
            }
        }

        private async Task CreateHistory(Commitment commitment, CallerType callerType, string userId, string userName)
        {
            var historyService = new HistoryService(_historyRepository);
            historyService.TrackDelete(commitment, CommitmentChangeType.Deleted.ToString(), commitment.Id, null, callerType, userId, commitment.ProviderId, commitment.EmployerAccountId, userName);
            await historyService.Save();
        }

        private static void CheckAuthorization(DeleteCommitmentCommand message, Commitment commitment)
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
            if (commitment.CommitmentStatus != CommitmentStatus.New && commitment.CommitmentStatus != CommitmentStatus.Active)
                throw new InvalidOperationException($"Commitment {commitment.Id} cannot be deleted because status is {commitment.CommitmentStatus}");
        }

        private static void CheckEditStatus(DeleteCommitmentCommand message, Commitment commitment)
        {
            switch (message.Caller.CallerType)
            {
                case CallerType.Provider:
                    if (commitment.EditStatus != EditStatus.Both && commitment.EditStatus != EditStatus.ProviderOnly)
                        throw new UnauthorizedException($"Provider {message.Caller.Id} not allowed to delete commitment {message.CommitmentId} because it is not with the provider");
                    break;
                case CallerType.Employer:
                    if (commitment.EditStatus != EditStatus.Both && commitment.EditStatus != EditStatus.EmployerOnly)
                        throw new UnauthorizedException($"Employer {message.Caller.Id} not allowed to delete commitment {message.CommitmentId} because it is not with the employer");
                    break;
            }
        }

        private static void CheckPaymentStatus(IEnumerable<Apprenticeship> apprenticeships)
        {
            var notPendingApprovalApprenticeships =
                apprenticeships
                .Where(apprenticeship => apprenticeship.PaymentStatus != PaymentStatus.PendingApproval)
                .Select(apprenticeship => apprenticeship.Id)
                .ToList();

            if (notPendingApprovalApprenticeships.Any())
                throw new UnauthorizedException($"Commitment cannot be deleted it contains an apprenticeship with payment status that is not PendingApproval: {string.Join(",", notPendingApprovalApprenticeships) }");
        }

        private void LogMessage(DeleteCommitmentCommand command)
        {
            string messageTemplate = $"{command.Caller.CallerType}: {command.Caller.Id} has called DeleteCommitmentCommand";

            if (command.Caller.CallerType == CallerType.Employer)
                _logger.Info(messageTemplate, accountId: command.Caller.Id, commitmentId: command.CommitmentId);
            else
                _logger.Info(messageTemplate, providerId: command.Caller.Id, commitmentId: command.CommitmentId);
        }

        private async Task PublishV2Events(Commitment commitment)
        {
            foreach (var apprenticeship in commitment.Apprenticeships)
            {
                await _v2EventsPublisher.PublishApprenticeshipDeleted(commitment, apprenticeship);
            }
        }

        private async Task PublishMessageIfChangeOfPartyCohortRejected(Commitment commitment, CallerType caller)
        {
            if (caller == CallerType.Provider && commitment.ChangeOfPartyRequestId.HasValue)
            {
                await _v2EventsPublisher.PublishProviderRejectedChangeOfPartyCohort(commitment);
            }
        }
    }
}
