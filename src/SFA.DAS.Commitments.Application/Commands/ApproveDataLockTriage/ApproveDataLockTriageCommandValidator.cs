using FluentValidation;

namespace SFA.DAS.Commitments.Application.Commands.ApproveDataLockTriage
{
    public class ApproveDataLockTriageCommandValidator : AbstractValidator<ApproveDataLockTriageCommand>
    {
        public ApproveDataLockTriageCommandValidator()
        {
            RuleFor(x => x.ApprenticeshipId).NotEmpty();
            RuleFor(x => x.UserId).NotEmpty();
        }
    }
}
