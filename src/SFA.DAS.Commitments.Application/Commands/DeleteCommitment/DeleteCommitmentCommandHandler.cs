using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;

using SFA.DAS.Commitments.Application.Exceptions;
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

        private readonly IHistoryRepository _historyRepository;

        public DeleteCommitmentCommandHandler(ICommitmentRepository commitmentRepository, AbstractValidator<DeleteCommitmentCommand> validator, ICommitmentsLogger logger, IHistoryRepository historyRepository)
        {
            if (commitmentRepository == null)
                throw new ArgumentNullException(nameof(commitmentRepository));
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (historyRepository == null)
                throw new ArgumentNullException(nameof(historyRepository));

            _commitmentRepository = commitmentRepository;
            _validator = validator;
            _logger = logger;
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

            await _historyRepository.CreateCommitmentHistory(
                new CommitmentHistoryItem
                    {
                        CommitmentId = commitment.Id,
                        UserId = command.UserId,
                        UpdatedByRole = command.Caller.CallerType
                    });
        }

        private static void CheckAuthorization(DeleteCommitmentCommand message, Commitment commitment)
        {
            switch (message.Caller.CallerType)
            {
                case CallerType.Provider:
                    if (commitment.ProviderId != message.Caller.Id)
                        throw new UnauthorizedException($"Provider {message.Caller.Id} unauthorized to delete commitment: {message.CommitmentId}");
                    break;
                case CallerType.Employer:
                default:
                    if (commitment.EmployerAccountId != message.Caller.Id)
                        throw new UnauthorizedException($"Employer {message.Caller.Id} unauthorized to delete commitment: {message.CommitmentId}");
                    break;
            }
        }

        private static void CheckCommitmentStatus(Commitment commitment)
        {
            if (commitment.CommitmentStatus != CommitmentStatus.New && commitment.CommitmentStatus != CommitmentStatus.Active)
                throw new InvalidOperationException($"Commitment {commitment.Id} cannot be delete because status is {commitment.CommitmentStatus}");
        }

        private static void CheckEditStatus(DeleteCommitmentCommand message, Commitment commitment)
        {
            switch (message.Caller.CallerType)
            {
                case CallerType.Provider:
                    if (commitment.EditStatus != EditStatus.Both && commitment.EditStatus != EditStatus.ProviderOnly)
                        throw new UnauthorizedException($"Provider {message.Caller.Id} unauthorized to edit commitment {message.CommitmentId}");
                    break;
                case CallerType.Employer:
                    if (commitment.EditStatus != EditStatus.Both && commitment.EditStatus != EditStatus.EmployerOnly)
                        throw new UnauthorizedException($"Employer {message.Caller.Id} unauthorized to edit commitment {message.CommitmentId}");
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
