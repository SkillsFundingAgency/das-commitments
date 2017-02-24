using FluentValidation;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.Queries.GetRelationship
{
    public sealed class GetRelationshipValidator : AbstractValidator<GetRelationshipRequest>
    {
        public GetRelationshipValidator()
        {
            RuleFor(x => x.EmployerAccountId)
                .NotEmpty()
                .WithMessage("EmployerAccountId must be specified");

            RuleFor(x => x.ProviderId)
                .NotEmpty()
                .WithMessage("ProviderId must be specified");

            RuleFor(x => x.LegalEntityId)
                .NotEmpty()
                .WithMessage("LegalEntityId must be specified");
        }
    }
}
