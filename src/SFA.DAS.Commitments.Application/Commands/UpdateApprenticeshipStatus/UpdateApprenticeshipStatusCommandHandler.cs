using System;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using NLog;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStatus
{
    public sealed class UpdateApprenticeshipStatusCommandHandler : AsyncRequestHandler<UpdateApprenticeshipStatusCommand>
    {
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly UpdateApprenticeshipStatusValidator _validator;
        private readonly ICommitmentsLogger _logger;

        public UpdateApprenticeshipStatusCommandHandler(ICommitmentRepository commitmentRepository, UpdateApprenticeshipStatusValidator validator, ICommitmentsLogger logger)
        {
            _commitmentRepository = commitmentRepository;
            _validator = validator;
            _logger = logger;
        }

        protected override async Task HandleCore(UpdateApprenticeshipStatusCommand command)
        {
            _logger.Info($"Employer: {command.AccountId} has called UpdateApprenticeshipStatusCommand", accountId: command.AccountId, commitmentId: command.CommitmentId, apprenticeshipId: command.ApprenticeshipId);

            var validationResult = _validator.Validate(command);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var commitment = await _commitmentRepository.GetCommitmentById(command.CommitmentId);

            CheckAuthorization(command, commitment);

            var apprenticeship = await _commitmentRepository.GetApprenticeship(command.ApprenticeshipId);
            var newPaymentStatus = (PaymentStatus) command.PaymentStatus.GetValueOrDefault((Api.Types.PaymentStatus) apprenticeship.PaymentStatus);

            await _commitmentRepository.UpdateApprenticeshipStatus(command.CommitmentId, command.ApprenticeshipId, newPaymentStatus);
        }

        private static void CheckAuthorization(UpdateApprenticeshipStatusCommand message, Commitment commitment)
        {
            if (commitment.EmployerAccountId != message.AccountId)
                throw new UnauthorizedException($"Employer {message.AccountId} unauthorized to view commitment {message.CommitmentId}");
        }
    }
}
