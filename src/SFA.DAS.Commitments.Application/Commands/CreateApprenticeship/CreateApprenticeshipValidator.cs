using FluentValidation;

namespace SFA.DAS.Commitments.Application.Commands.CreateApprenticeship
{
    public sealed class CreateApprenticeshipValidator : AbstractValidator<CreateApprenticeshipCommand>
    {
        public CreateApprenticeshipValidator()
        {
            RuleFor(x => x.Apprenticeship).NotNull();
            RuleFor(x => x.CommitmentId).GreaterThan(0);
            RuleFor(x => x.ProviderId).GreaterThan(0);
        }
    }
}
