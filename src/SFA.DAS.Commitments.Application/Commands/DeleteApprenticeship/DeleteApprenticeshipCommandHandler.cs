﻿using System;
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

namespace SFA.DAS.Commitments.Application.Commands.DeleteApprenticeship
{
    public sealed class DeleteApprenticeshipCommandHandler : AsyncRequestHandler<DeleteApprenticeshipCommand>
    {
        private readonly ICommitmentRepository _commitmentRepository;

        private readonly IApprenticeshipRepository _apprenticeshipRepository;

        private readonly AbstractValidator<DeleteApprenticeshipCommand> _validator;
        private readonly ICommitmentsLogger _logger;
        private readonly IApprenticeshipEvents _apprenticeshipEvents;
        private readonly IHistoryRepository _historyRepository;

        public DeleteApprenticeshipCommandHandler(ICommitmentRepository commitmentRepository, IApprenticeshipRepository apprenticeshipRepository, AbstractValidator<DeleteApprenticeshipCommand> validator, ICommitmentsLogger logger, IApprenticeshipEvents apprenticeshipEvents, IHistoryRepository historyRepository)
        {
            _commitmentRepository = commitmentRepository;
            _apprenticeshipRepository = apprenticeshipRepository;
            _validator = validator;
            _logger = logger;
            _apprenticeshipEvents = apprenticeshipEvents;
            _historyRepository = historyRepository;
        }

        protected override async Task HandleCore(DeleteApprenticeshipCommand command)
        {
            LogMessage(command);

            var validationResult = _validator.Validate(command);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var apprenticeship = await _apprenticeshipRepository.GetApprenticeship(command.ApprenticeshipId);

            if (apprenticeship == null)
            {
                throw new ResourceNotFoundException();
            }

            var commitment = await _commitmentRepository.GetCommitmentById(apprenticeship.CommitmentId);

            CheckAuthorization(command, apprenticeship);
            CheckCommitmentStatus(command, commitment);
            CheckEditStatus(command, commitment);
            CheckPaymentStatus(apprenticeship);

            await Task.WhenAll(
                _apprenticeshipRepository.DeleteApprenticeship(command.ApprenticeshipId),
                 _apprenticeshipEvents.PublishDeletionEvent(commitment, apprenticeship, "APPRENTICESHIP-DELETED"),
                CreateHistory(commitment, command.Caller.CallerType, command.UserId, command.UserName)
            );
        }

        private async Task CreateHistory(Commitment commitment, CallerType callerType, string userId, string userName)
        {
            var commitmentHistory = new HistoryService(_historyRepository);
            commitmentHistory.TrackUpdate(commitment, CommitmentChangeType.DeletedApprenticeship.ToString(), commitment.Id, null, callerType, userId, commitment.ProviderId, commitment.EmployerAccountId, userName);
            await commitmentHistory.Save();
        }

        private void LogMessage(DeleteApprenticeshipCommand command)
        {
            string messageTemplate = $"{command.Caller.CallerType}: {command.Caller.Id} has called DeleteApprenticeshipCommand";

            if (command.Caller.CallerType == CallerType.Employer)
                _logger.Info(messageTemplate, accountId: command.Caller.Id, apprenticeshipId: command.ApprenticeshipId);
            else
                _logger.Info(messageTemplate, providerId: command.Caller.Id, apprenticeshipId: command.ApprenticeshipId);
        }

        private static void CheckCommitmentStatus(DeleteApprenticeshipCommand message, Commitment commitment)
        {
            if (commitment.CommitmentStatus != CommitmentStatus.New && commitment.CommitmentStatus != CommitmentStatus.Active)
                throw new InvalidOperationException($"Apprenticeship {message.ApprenticeshipId} in commitment {commitment.Id} cannot be updated because status is {commitment.CommitmentStatus}");
        }

        private static void CheckEditStatus(DeleteApprenticeshipCommand message, Commitment commitment)
        {
            switch (message.Caller.CallerType)
            {
                case CallerType.Provider:
                    if (commitment.EditStatus != EditStatus.Both && commitment.EditStatus != EditStatus.ProviderOnly)
                        throw new UnauthorizedException($"Provider {message.Caller.Id} not allowed to edit apprenticeship {message.ApprenticeshipId} in commitment {commitment.Id}");
                    break;
                case CallerType.Employer:
                    if (commitment.EditStatus != EditStatus.Both && commitment.EditStatus != EditStatus.EmployerOnly)
                        throw new UnauthorizedException($"Employer {message.Caller.Id} not allowed to edit apprenticeship {message.ApprenticeshipId} in commitment {commitment.Id}");
                    break;
            }
        }

        private static void CheckPaymentStatus(Apprenticeship apprenticeship)
        {
            var allowedPaymentStatusesForDeleting = new[] {PaymentStatus.PendingApproval};

            if (!allowedPaymentStatusesForDeleting.Contains(apprenticeship.PaymentStatus))
                throw new UnauthorizedException($"Apprenticeship {apprenticeship.Id} cannot be deleted when payment status is {apprenticeship.PaymentStatus}");
        }

        private static void CheckAuthorization(DeleteApprenticeshipCommand message, Apprenticeship apprenticeship)
        {
            switch (message.Caller.CallerType)
            {
                case CallerType.Provider:
                    if (apprenticeship.ProviderId != message.Caller.Id)
                        throw new UnauthorizedException($"Provider {message.Caller.Id} not authorised to access apprenticeship: {message.ApprenticeshipId}, expected provider {apprenticeship.ProviderId}");
                    break;
                case CallerType.Employer:
                default:
                    if (apprenticeship.EmployerAccountId != message.Caller.Id)
                        throw new UnauthorizedException($"Employer {message.Caller.Id} not authorised to access apprenticeship: {message.ApprenticeshipId}, expected employer {apprenticeship.EmployerAccountId}");
                    break;
            }
        }
    }
}
