using FluentValidation;

namespace SFA.DAS.Commitments.Application.Commands.CreateCommitment
{
    public sealed class CreateCommitmentValidator : AbstractValidator<CreateCommitmentCommand>
    {
        public CreateCommitmentValidator()
        {
            RuleFor(x => x.Commitment.Name).NotNull().NotEmpty();
            RuleFor(x => x.Commitment.EmployerAccountId).GreaterThan(0);
            RuleFor(x => x.Commitment.LegalEntityId).GreaterThan(0);
            RuleFor(x => x.Commitment.Apprenticeships).NotEmpty();
            RuleFor(x => x.Commitment.ProviderId).Must(x => !x.HasValue || x.Value > 0);
        }
    }
}
