using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;

namespace SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStatus
{
    public sealed class UpdateApprenticeshipStatusCommandHandler : AsyncRequestHandler<UpdateApprenticeshipStatusCommand>
    {
        private ICommitmentRepository _commitmentRepository;
        private UpdateApprenticeshipStatusValidator _validator;
        private IValidateStateTransition<ApprenticeshipStatus> _stateTransitionValidator;

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

            var apprenticeship = await _commitmentRepository.GetApprenticeship(message.ApprenticeshipId);

            if (!_stateTransitionValidator.IsStateTransitionValid(apprenticeship.Status, (ApprenticeshipStatus)message.Status))
                throw new InvalidRequestException();

            await _commitmentRepository.UpdateApprenticeshipStatus(message.CommitmentId, message.ApprenticeshipId, (ApprenticeshipStatus)message.Status);
        }
    }
}
