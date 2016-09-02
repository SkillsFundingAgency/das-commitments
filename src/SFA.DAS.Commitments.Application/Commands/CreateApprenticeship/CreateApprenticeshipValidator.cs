using FluentValidation;

namespace SFA.DAS.Commitments.Application.Commands.CreateApprenticeship
{
    public sealed class CreateApprenticeshipValidator : AbstractValidator<CreateApprenticeshipCommand>
    {
        public CreateApprenticeshipValidator()
        {
            RuleFor(x => x.Apprenticeship).NotNull();
        }
    }
}
