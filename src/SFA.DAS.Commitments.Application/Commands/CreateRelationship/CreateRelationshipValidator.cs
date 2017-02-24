using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace SFA.DAS.Commitments.Application.Commands.CreateRelationship
{
    public sealed class CreateRelationshipValidator : AbstractValidator<CreateRelationshipCommand>
    {
        public CreateRelationshipValidator()
        {
            RuleFor(x => x.Relationship).NotNull().DependentRules(y =>
            {
                y.RuleFor(x => x.Relationship.ProviderId).GreaterThan(0);
                y.RuleFor(x => x.Relationship.ProviderName).NotNull().NotEmpty();
                y.RuleFor(x => x.Relationship.EmployerAccountId).GreaterThan(0);
                y.RuleFor(x => x.Relationship.LegalEntityId).NotEmpty();
                y.RuleFor(x => x.Relationship.LegalEntityName).NotEmpty();
                y.RuleFor(x => x.Relationship.ProviderName).NotNull().NotEmpty();
            });
        }
    }
}
