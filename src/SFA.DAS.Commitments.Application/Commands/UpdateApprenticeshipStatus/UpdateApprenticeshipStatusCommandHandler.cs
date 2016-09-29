using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using NLog;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;

namespace SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStatus
{
    public sealed class UpdateApprenticeshipStatusCommandHandler : AsyncRequestHandler<UpdateApprenticeshipStatusCommand>
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly UpdateApprenticeshipStatusValidator _validator;
        private readonly IValidateStateTransition<ApprenticeshipStatus> _stateTransitionValidator;

        public UpdateApprenticeshipStatusCommandHandler(ICommitmentRepository commitmentRepository, UpdateApprenticeshipStatusValidator validator, IValidateStateTransition<ApprenticeshipStatus> stateTransitionValidator)
        {
            _commitmentRepository = commitmentRepository;
            _validator = validator;
            _stateTransitionValidator = stateTransitionValidator;
        }

        protected override async Task HandleCore(UpdateApprenticeshipStatusCommand message)
        {
            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var commitment = await _commitmentRepository.GetById(message.CommitmentId);

            CheckAuthorization(message, commitment);

            var apprenticeship = await _commitmentRepository.GetApprenticeship(message.ApprenticeshipId);

            if (!_stateTransitionValidator.IsStateTransitionValid(apprenticeship.Status, (ApprenticeshipStatus)message.Status))
                throw new InvalidRequestException();

            await _commitmentRepository.UpdateApprenticeshipStatus(message.CommitmentId, message.ApprenticeshipId, (ApprenticeshipStatus)message.Status);
        }

        private static void CheckAuthorization(UpdateApprenticeshipStatusCommand message, Domain.Commitment commitment)
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
