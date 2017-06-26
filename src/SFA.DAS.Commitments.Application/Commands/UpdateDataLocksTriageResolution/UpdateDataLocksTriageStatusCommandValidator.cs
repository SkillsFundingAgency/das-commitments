using FluentValidation;

namespace SFA.DAS.Commitments.Application.Commands.UpdateDataLocksTriageResolution
{
    public sealed class UpdateDataLocksTriageStatusResolutionCommandValidator : AbstractValidator<UpdateDataLocksTriageResolutionCommand>
    {
        public UpdateDataLocksTriageStatusResolutionCommandValidator()
        {
            RuleFor(x => x.ApprenticeshipId).NotEmpty();
            RuleFor(x => x.DataLockUpdateType).IsInEnum();
            RuleFor(x => x.UserId).NotEmpty();
        }
    }
}