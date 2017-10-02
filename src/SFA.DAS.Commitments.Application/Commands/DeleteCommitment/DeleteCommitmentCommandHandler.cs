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

namespace SFA.DAS.Commitments.Application.Commands.DeleteCommitment
{
    public sealed class DeleteCommitmentCommandHandler : AsyncRequestHandler<DeleteCommitmentCommand>
    {
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly AbstractValidator<DeleteCommitmentCommand> _validator;
        private readonly ICommitmentsLogger _logger;
        private readonly IApprenticeshipEvents _apprenticeshipEvents;
        private readonly IHistoryRepository _historyRepository;

        public DeleteCommitmentCommandHandler(ICommitmentRepository commitmentRepository, AbstractValidator<DeleteCommitmentCommand> validator, ICommitmentsLogger logger, IApprenticeshipEvents apprenticeshipEvents, IHistoryRepository historyRepository)
        {
            _commitmentRepository = commitmentRepository;
            _validator = validator;
            _logger = logger;
            _apprenticeshipEvents = apprenticeshipEvents;
            _historyRepository = historyRepository;
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
        }

        private async Task CreateHistory(Commitment commitment, CallerType callerType, string userId, string userName)
        {
            var historyService = new HistoryService(_historyRepository);
            historyService.TrackDelete(commitment, CommitmentChangeType.Deleted.ToString(), commitment.Id, null, callerType, userId, userName);
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
                        throw new UnauthorizedException($"Provider {message.Caller.Id} not allowed to delete commitment {message.CommitmentId}, expected provider {commitment.ProviderId}");
                    break;
                case CallerType.Employer:
                    if (commitment.EditStatus != EditStatus.Both && commitment.EditStatus != EditStatus.EmployerOnly)
                        throw new UnauthorizedException($"Employer {message.Caller.Id} not allowed to delete commitment {message.CommitmentId}, expected employer {commitment.EmployerAccountId}");
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
    }
}
