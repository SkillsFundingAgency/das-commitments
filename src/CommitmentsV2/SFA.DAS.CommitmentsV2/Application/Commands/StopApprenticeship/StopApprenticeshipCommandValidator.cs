using FluentValidation;
using SFA.DAS.CommitmentsV2.Application.Commands.StopApprenticeship;
using System;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AddCohort
{
    public class StopApprenticeshipCommandValidator : AbstractValidator<StopApprenticeshipCommand>
    {
        public StopApprenticeshipCommandValidator()
        {
            RuleFor(model => model.AccountId).Must(id => id > 0).WithMessage("The Account Id must be positive");
            RuleFor(model => model.ApprenticeshipId).Must(id => id > 0).WithMessage("The ApprenticeshipId must be positive");
            RuleFor(model => model.UserInfo).Must(info => info != null && info.UserId != null).WithMessage("The User Info supplied must not be null and contain a UserId");
            RuleFor(model => model.StopDate).Must(date => date != DateTime.MinValue).WithMessage("The StopDate must be supplied");
        }
    }
}
