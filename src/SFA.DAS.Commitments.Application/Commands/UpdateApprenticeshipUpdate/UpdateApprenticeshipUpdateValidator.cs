using FluentValidation;

namespace SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipUpdate
{
    public class UpdateApprenticeshipUpdateValidator : AbstractValidator<UpdateApprenticeshipUpdateCommand>
    {
        public UpdateApprenticeshipUpdateValidator()
        {
            RuleFor(x => x.ApprenticeshipId).NotNull();
        }
    }
}