using FluentValidation;

namespace SFA.DAS.Commitments.Application.Commands.VerifyRelationship
{
    public class VerifyRelationshipValidator : AbstractValidator<VerifyRelationshipCommand>
    {
        public VerifyRelationshipValidator()
        {
            RuleFor(x => x.EmployerAccountId).NotEmpty();
            RuleFor(x => x.ProviderId).NotEmpty();
            RuleFor(x => x.LegalEntityId).NotEmpty();
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.Verified).NotEmpty();
        }
    }
}
