using System;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using NLog;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStatus
{
    public sealed class UpdateApprenticeshipStatusCommandHandler : AsyncRequestHandler<UpdateApprenticeshipStatusCommand>
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly UpdateApprenticeshipStatusValidator _validator;

        public UpdateApprenticeshipStatusCommandHandler(ICommitmentRepository commitmentRepository, UpdateApprenticeshipStatusValidator validator)
        {
            _commitmentRepository = commitmentRepository;
            _validator = validator;
        }

        protected override async Task HandleCore(UpdateApprenticeshipStatusCommand message)
        {
            Logger.Info($"Employer: {message.AccountId} has called UpdateApprenticeshipStatusCommand");

            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var commitment = await _commitmentRepository.GetById(message.CommitmentId);

            CheckAuthorization(message, commitment);

            var apprenticeship = await _commitmentRepository.GetApprenticeship(message.ApprenticeshipId);
            var newPaymentStatus = (PaymentStatus) message.PaymentStatus.GetValueOrDefault((Api.Types.PaymentStatus) apprenticeship.PaymentStatus);

            await _commitmentRepository.UpdateApprenticeshipStatus(message.CommitmentId, message.ApprenticeshipId, newPaymentStatus);
        }

        private static void CheckAuthorization(UpdateApprenticeshipStatusCommand message, Commitment commitment)
        {
            if (commitment.EmployerAccountId != message.AccountId)
                throw new UnauthorizedException($"Employer unauthorized to view commitment: {message.CommitmentId}");
        }
    }
}
