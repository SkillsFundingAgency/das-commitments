using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStatus
{
    public sealed class UpdateApprenticeshipStatusCommandHandler : AsyncRequestHandler<UpdateApprenticeshipStatusCommand>
    {
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly UpdateApprenticeshipStatusValidator _validator;
        private readonly ICommitmentsLogger _logger;

        public UpdateApprenticeshipStatusCommandHandler(ICommitmentRepository commitmentRepository, IApprenticeshipRepository apprenticeshipRepository, UpdateApprenticeshipStatusValidator validator, ICommitmentsLogger logger)
        {
            _commitmentRepository = commitmentRepository;
            _apprenticeshipRepository = apprenticeshipRepository;
            _validator = validator;
            _logger = logger;
        }

        protected override async Task HandleCore(UpdateApprenticeshipStatusCommand command)
        {
            _logger.Info($"Employer: {command.AccountId} has called UpdateApprenticeshipStatusCommand", accountId: command.AccountId, apprenticeshipId: command.ApprenticeshipId);

            var validationResult = _validator.Validate(command);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var apprenticeship = await _apprenticeshipRepository.GetApprenticeship(command.ApprenticeshipId);
            var commitment = await _commitmentRepository.GetCommitmentById(apprenticeship.CommitmentId);
            CheckAuthorization(command, commitment);

            var newPaymentStatus = (PaymentStatus)command.PaymentStatus.GetValueOrDefault((Api.Types.Apprenticeship.Types.PaymentStatus)apprenticeship.PaymentStatus);

            // TODO: LWA - validate that date for change is valid value
            ValidateDateOfChange(command.PaymentStatus, command.DateOfChange, apprenticeship);

            await SaveChange(command, commitment, newPaymentStatus);
        }

        private void ValidateDateOfChange(Api.Types.Apprenticeship.Types.PaymentStatus? paymentStatus, DateTime dateOfChange, Apprenticeship apprenticeship)
        {
            return;
        }

        private async Task SaveChange(UpdateApprenticeshipStatusCommand command, Commitment commitment, PaymentStatus newPaymentStatus)
        {
            if (newPaymentStatus == PaymentStatus.Cancelled) // TODO: LWA - Rename this payment status
            {
                await _apprenticeshipRepository.StopApprenticeship(commitment.Id, command.ApprenticeshipId, command.DateOfChange);

                return;
            }

            throw new NotImplementedException();
        }

        private static void CheckAuthorization(UpdateApprenticeshipStatusCommand message, Commitment commitment)
        {
            if (commitment.EmployerAccountId != message.AccountId)
                throw new UnauthorizedException($"Employer {message.AccountId} unauthorized to view commitment {commitment.Id}");
        }
    }
}
