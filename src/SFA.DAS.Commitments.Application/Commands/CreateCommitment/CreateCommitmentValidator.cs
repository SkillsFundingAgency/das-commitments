using FluentValidation;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Application.Commands.CreateApprenticeship;

namespace SFA.DAS.Commitments.Application.Commands.CreateCommitment
{
    public sealed class CreateCommitmentValidator : AbstractValidator<CreateCommitmentCommand>
    {
        public CreateCommitmentValidator(AbstractValidator<Apprenticeship> apprenticeshipValidator)
        {
            // TODO: LWA inject the apprenticeship validator
            RuleFor(x => x.Commitment).NotNull().DependentRules(y =>
            {
                y.RuleFor(x => x.Commitment.Reference).NotNull().NotEmpty();
                y.RuleFor(x => x.Commitment.EmployerAccountId).GreaterThan(0);
                y.RuleFor(x => x.Commitment.LegalEntityId).NotEmpty();
                y.RuleFor(x => x.Commitment.ProviderId).Must(x => !x.HasValue || x.Value > 0);
                y.RuleFor(x => x.Commitment.Apprenticeships).SetCollectionValidator(apprenticeshipValidator);
            });
        }
    }
}
