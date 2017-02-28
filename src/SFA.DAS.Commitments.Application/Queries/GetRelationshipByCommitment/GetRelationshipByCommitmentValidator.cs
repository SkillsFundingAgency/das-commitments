using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace SFA.DAS.Commitments.Application.Queries.GetRelationshipByCommitment
{
    public sealed class GetRelationshipByCommitmentValidator : AbstractValidator<GetRelationshipByCommitmentRequest>
    {
        public GetRelationshipByCommitmentValidator()
        {
            RuleFor(x => x.ProviderId)
                .NotEmpty()
                .WithMessage("ProviderId must be specified");

            RuleFor(x => x.CommitmentId)
                .NotEmpty()
                .WithMessage("CommitmentId must be specified");
        }
    }
}
