using System;
using FluentValidation;

namespace SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStopDate
{
    public sealed class UpdateApprenticeshipStopDateCommandValidator : AbstractValidator<UpdateApprenticeshipStopDateCommand>
    {
        public UpdateApprenticeshipStopDateCommandValidator()
        {
            RuleFor(x => x.AccountId).GreaterThan(0);
            RuleFor(x => x.ApprenticeshipId).GreaterThan(0);
            RuleFor(x => x.StopDate).GreaterThan(DateTime.MinValue);
        }
    }
}