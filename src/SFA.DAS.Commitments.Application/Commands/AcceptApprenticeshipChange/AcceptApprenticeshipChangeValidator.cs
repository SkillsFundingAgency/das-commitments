using FluentValidation;

namespace SFA.DAS.Commitments.Application.Commands.AcceptApprenticeshipChange
{
    public class AcceptApprenticeshipChangeValidator : AbstractValidator<AcceptApprenticeshipChangeCommand>
    {
        public AcceptApprenticeshipChangeValidator()
        {
            RuleFor(x => x.ApprenticeshipId).NotNull();
        }
    }
}
