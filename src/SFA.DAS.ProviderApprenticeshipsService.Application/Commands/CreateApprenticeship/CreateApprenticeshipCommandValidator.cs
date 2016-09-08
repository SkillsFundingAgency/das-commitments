using FluentValidation;

namespace SFA.DAS.ProviderApprenticeshipsService.Application.Commands.CreateApprenticeship
{
    public class CreateApprenticeshipCommandValidator : AbstractValidator<CreateApprenticeshipCommand>
    {
        public CreateApprenticeshipCommandValidator()
        {
            RuleFor(x => x.ProviderId).GreaterThan(0);
            RuleFor(x => x.Apprenticeship.CommitmentId).GreaterThan(0);
        }
    }
}