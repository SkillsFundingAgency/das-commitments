using System;
using FluentValidation;
using SFA.DAS.Commitments.Api.Types;

namespace SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStatus
{
    public sealed class UpdateApprenticeshipStatusValidator : AbstractValidator<UpdateApprenticeshipStatusCommand>
    {
        public UpdateApprenticeshipStatusValidator()
        {
            RuleFor(x => x.AccountId).GreaterThan(0);
            RuleFor(x => x.CommitmentId).GreaterThan(0);
            RuleFor(x => x.ApprenticeshipId).GreaterThan(0);
            RuleFor(x => x.Status).NotNull()
                .DependentRules(y => y.RuleFor(z => z.Status).Must(a => Enum.IsDefined(typeof(ApprenticeshipStatus), (short)a)));
        }
    }
}
