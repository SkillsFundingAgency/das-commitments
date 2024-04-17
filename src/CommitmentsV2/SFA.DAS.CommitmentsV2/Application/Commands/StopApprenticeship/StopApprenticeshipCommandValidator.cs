using FluentValidation;
using System;

namespace SFA.DAS.CommitmentsV2.Application.Commands.StopApprenticeship
{
    public class StopApprenticeshipCommandValidator : AbstractValidator<StopApprenticeshipCommand>
    {
        public StopApprenticeshipCommandValidator()
        {
            RuleFor(model => model.AccountId).Must(id => id > 0).WithMessage("The Account Id must be positive");
            RuleFor(model => model.ApprenticeshipId).Must(id => id > 0).WithMessage("The ApprenticeshipId must be positive");
            RuleFor(model => model.UserInfo).Must(info => info != null && info.UserId != null).WithMessage("The User Info supplied must not be null and contain a UserId");
            RuleFor(model => model.StopDate).Must(date => date != DateTime.MinValue).WithMessage("The StopDate must be supplied");
            RuleFor(model => model.Party).Must(party => party != Types.Party.None).WithMessage("The Party must be supplied");
        }
    }
}
