using System;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.Commands.DeleteApprenticeship
{
    public sealed class DeleteApprenticeshipCommandHandler : AsyncRequestHandler<DeleteApprenticeshipCommand>
    {
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly AbstractValidator<DeleteApprenticeshipCommand> _validator;
        private readonly ICommitmentsLogger _logger;

        public DeleteApprenticeshipCommandHandler(ICommitmentRepository commitmentRepository, AbstractValidator<DeleteApprenticeshipCommand> validator, ICommitmentsLogger logger)
        {
            _commitmentRepository = commitmentRepository;
            _validator = validator;
            _logger = logger;
        }

        protected override async Task HandleCore(DeleteApprenticeshipCommand command)
        {
            LogMessage(command);

            var validationResult = _validator.Validate(command);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var apprenticeship = await _commitmentRepository.GetApprenticeship(command.ApprenticeshipId);

            if (apprenticeship == null)
            {
                throw new ResourceNotFoundException();
            }

            var commitment = await _commitmentRepository.GetCommitmentById(apprenticeship.CommitmentId);

            CheckAuthorization(command, apprenticeship);
            CheckCommitmentStatus(command, commitment);
            CheckEditStatus(command, commitment);
            CheckPaymentStatus(apprenticeship);

            await _commitmentRepository.DeleteApprenticeship(command.ApprenticeshipId);
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
                        throw new UnauthorizedException($"Provider {message.Caller.Id} unauthorized to edit apprenticeship {message.ApprenticeshipId} in commitment {commitment.Id}");
                    break;
                case CallerType.Employer:
                    if (commitment.EditStatus != EditStatus.Both && commitment.EditStatus != EditStatus.EmployerOnly)
                        throw new UnauthorizedException($"Employer {message.Caller.Id} unauthorized to edit apprenticeship {message.ApprenticeshipId} in commitment {commitment.Id}");
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
                        throw new UnauthorizedException($"Provider {message.Caller.Id} unauthorized to view apprenticeship: {message.ApprenticeshipId}");
                    break;
                case CallerType.Employer:
                default:
                    if (apprenticeship.EmployerAccountId != message.Caller.Id)
                        throw new UnauthorizedException($"Employer {message.Caller.Id} unauthorized to view apprenticeship: {message.ApprenticeshipId}");
                    break;
            }
        }
    }
}
