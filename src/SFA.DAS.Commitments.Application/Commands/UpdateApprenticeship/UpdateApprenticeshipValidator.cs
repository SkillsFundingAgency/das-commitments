using FluentValidation;

namespace SFA.DAS.Commitments.Application.Commands.UpdateApprenticeship
{
    public sealed class UpdateApprenticeshipValidator : AbstractValidator<UpdateApprenticeshipCommand>
    {
        public UpdateApprenticeshipValidator()
        {
            RuleFor(x => x.Apprenticeship).NotNull();
            RuleFor(x => x.ApprenticeshipId).GreaterThan(0);
            RuleFor(x => x.CommitmentId).GreaterThan(0);
            RuleFor(x => x.ProviderId).GreaterThan(0);
        }
    }
}
