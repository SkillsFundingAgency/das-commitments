using FluentValidation;

namespace SFA.DAS.Commitments.Application.Commands.UpdateDataLockTriageStatus
{
    public sealed class UpdateDataLockTriageStatusCommandValidator : AbstractValidator<UpdateDataLockTriageStatusCommand>
    {
        public UpdateDataLockTriageStatusCommandValidator()
        {
            RuleFor(x => x.ApprenticeshipId).NotEmpty();
            RuleFor(x => x.DataLockEventId).NotEmpty();
            RuleFor(x => x.TriageStatus).IsInEnum();
            RuleFor(x => x.UserId).NotEmpty();
        }
    }
}
