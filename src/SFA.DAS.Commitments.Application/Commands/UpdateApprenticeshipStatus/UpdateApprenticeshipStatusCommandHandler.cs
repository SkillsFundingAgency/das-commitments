using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using NLog;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStatus
{
    public sealed class UpdateApprenticeshipStatusCommandHandler : AsyncRequestHandler<UpdateApprenticeshipStatusCommand>
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly UpdateApprenticeshipStatusValidator _validator;
        private readonly IValidateStateTransition<PaymentStatus> _stateTransitionValidator;

        public UpdateApprenticeshipStatusCommandHandler(ICommitmentRepository commitmentRepository, UpdateApprenticeshipStatusValidator validator, IValidateStateTransition<PaymentStatus> stateTransitionValidator)
        {
            _commitmentRepository = commitmentRepository;
            _validator = validator;
            _stateTransitionValidator = stateTransitionValidator;
        }

        protected override async Task HandleCore(UpdateApprenticeshipStatusCommand message)
        {
            Logger.Info(BuildInfoMessage(message));

            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var commitment = await _commitmentRepository.GetById(message.CommitmentId);

            CheckAuthorization(message, commitment);

            var apprenticeship = await _commitmentRepository.GetApprenticeship(message.ApprenticeshipId);

            if (!_stateTransitionValidator.IsStateTransitionValid(apprenticeship.PaymentStatus, (PaymentStatus)message.PaymentStatus))
                throw new InvalidRequestException();

            await _commitmentRepository.UpdateApprenticeshipStatus(message.CommitmentId, message.ApprenticeshipId, (PaymentStatus)message.PaymentStatus);
        }

        private static void CheckAuthorization(UpdateApprenticeshipStatusCommand message, Commitment commitment)
        {
            if (commitment.EmployerAccountId != message.AccountId)
                throw new UnauthorizedException($"Employer unauthorized to view commitment: {message.CommitmentId}");
        }

        private string BuildInfoMessage(UpdateApprenticeshipStatusCommand cmd)
        {
            return $"Employer: {cmd.AccountId} has called UpdateApprenticeshipStatusCommand";
        }
    }
}
