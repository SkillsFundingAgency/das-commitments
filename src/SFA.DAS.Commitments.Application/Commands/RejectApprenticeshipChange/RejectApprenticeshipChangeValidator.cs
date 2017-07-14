using FluentValidation;

namespace SFA.DAS.Commitments.Application.Commands.RejectApprenticeshipChange
{
    public class RejectApprenticeshipChangeValidator : AbstractValidator<RejectApprenticeshipChangeCommand>
    {
        public RejectApprenticeshipChangeValidator()
        {
            RuleFor(x => x.ApprenticeshipId).NotNull();
        }
    }
}
