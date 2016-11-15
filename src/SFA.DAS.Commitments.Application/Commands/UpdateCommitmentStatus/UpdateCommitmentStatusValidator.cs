using System;
using FluentValidation;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Commands.UpdateCommitmentStatus
{
    public sealed class UpdateCommitmentStatusValidator: AbstractValidator<UpdateCommitmentStatusCommand>
    {
        public UpdateCommitmentStatusValidator()
        {
            RuleFor(x => x.Caller.Id).GreaterThan(0);
            RuleFor(x => x.CommitmentId).GreaterThan(0);
            RuleFor(x => x.CommitmentStatus).NotNull()
                .DependentRules(y => y.RuleFor(z => z.CommitmentStatus).Must(a => Enum.IsDefined(typeof(CommitmentStatus), (short)a)));
        }
    }
}
