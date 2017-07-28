using FluentValidation;

namespace SFA.DAS.Commitments.Application.Commands.TriageDataLocks
{
    public sealed class TriageDataLockCommandValidator : AbstractValidator<TriageDataLockCommand>
    {
        public TriageDataLockCommandValidator()
        {
            RuleFor(x => x.ApprenticeshipId).NotEmpty();
            RuleFor(x => x.TriageStatus).IsInEnum();
            RuleFor(x => x.UserId).NotEmpty();
        }
    }
}