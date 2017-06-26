using FluentValidation;

namespace SFA.DAS.Commitments.Application.Commands.UpdateDataLocksTriageStatus
{
    public sealed class UpdateDataLocksTriageStatusCommandValidator : AbstractValidator<UpdateDataLocksTriageStatusCommand>
    {
        public UpdateDataLocksTriageStatusCommandValidator()
        {
            RuleFor(x => x.ApprenticeshipId).NotEmpty();
            RuleFor(x => x.TriageStatus).IsInEnum();
            RuleFor(x => x.UserId).NotEmpty();
        }
    }
}