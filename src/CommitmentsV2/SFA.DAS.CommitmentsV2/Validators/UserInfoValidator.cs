using FluentValidation;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Validators;

public class UserInfoValidator :  AbstractValidator<UserInfo>
{
    public UserInfoValidator()
    {
        RuleFor(model => model.UserId).NotNull().NotEmpty();
        RuleFor(model => model.UserDisplayName).NotNull().NotEmpty();
        RuleFor(model => model.UserEmail).NotNull().EmailAddress().WithMessage("The User Email address must be valid");
    }
}