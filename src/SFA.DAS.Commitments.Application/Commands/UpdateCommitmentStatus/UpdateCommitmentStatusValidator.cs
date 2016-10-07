using System;
using FluentValidation;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.Commands.UpdateCommitmentStatus
{
    public sealed class UpdateCommitmentStatusValidator: AbstractValidator<UpdateCommitmentStatusCommand>
    {
        public UpdateCommitmentStatusValidator()
        {
            RuleFor(x => x.Caller.Id).GreaterThan(0);
            RuleFor(x => x.CommitmentId).GreaterThan(0);
            RuleFor(x => x.Status).NotNull()
                .DependentRules(y => y.RuleFor(z => z.Status).Must(a => Enum.IsDefined(typeof(CommitmentStatus), (short)a)));
        }
    }
}
