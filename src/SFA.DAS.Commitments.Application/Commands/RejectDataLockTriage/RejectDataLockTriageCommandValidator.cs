using FluentValidation;

namespace SFA.DAS.Commitments.Application.Commands.RejectDataLockTriage
{
    public sealed class RejectDataLockTriageCommandValidator : AbstractValidator<RejectDataLockTriageCommand>
    {
        public RejectDataLockTriageCommandValidator()
        {
            RuleFor(x => x.ApprenticeshipId).NotEmpty();
            RuleFor(x => x.UserId).NotEmpty();
        }
    }
}