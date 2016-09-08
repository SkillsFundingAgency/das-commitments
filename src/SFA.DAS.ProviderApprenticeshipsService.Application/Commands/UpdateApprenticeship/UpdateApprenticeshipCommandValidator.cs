using FluentValidation;

namespace SFA.DAS.ProviderApprenticeshipsService.Application.Commands.UpdateApprenticeship
{
    public class UpdateApprenticeshipCommandValidator : AbstractValidator<UpdateApprenticeshipCommand>
    {
        public UpdateApprenticeshipCommandValidator()
        {
            RuleFor(x => x.ProviderId).GreaterThan(0);
            RuleFor(x => x.Apprenticeship.Id).GreaterThan(0);
            RuleFor(x => x.Apprenticeship.CommitmentId).GreaterThan(0);
        }
    }
}