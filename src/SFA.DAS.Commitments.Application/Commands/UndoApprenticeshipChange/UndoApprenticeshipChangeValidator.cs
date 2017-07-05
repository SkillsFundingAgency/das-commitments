using FluentValidation;

namespace SFA.DAS.Commitments.Application.Commands.UndoApprenticeshipChange
{
    public class UndoApprenticeshipChangeValidator : AbstractValidator<UndoApprenticeshipChangeCommand>
    {
        public UndoApprenticeshipChangeValidator()
        {
            RuleFor(x => x.ApprenticeshipId).NotNull();
        }
    }
}
