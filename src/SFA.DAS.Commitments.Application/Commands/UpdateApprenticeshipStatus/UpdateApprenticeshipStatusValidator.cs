using System;
using FluentValidation;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;

namespace SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStatus
{
    public sealed class UpdateApprenticeshipStatusValidator : AbstractValidator<UpdateApprenticeshipStatusCommand>
    {
        public UpdateApprenticeshipStatusValidator()
        {
            RuleFor(x => x.AccountId).GreaterThan(0);
            RuleFor(x => x.CommitmentId).GreaterThan(0);
            RuleFor(x => x.ApprenticeshipId).GreaterThan(0);
            RuleFor(x => x.PaymentStatus).NotNull()
                .DependentRules(y => y.RuleFor(z => z.PaymentStatus).Must(a => Enum.IsDefined(typeof(PaymentStatus), (short)a)));
        }
    }
}
