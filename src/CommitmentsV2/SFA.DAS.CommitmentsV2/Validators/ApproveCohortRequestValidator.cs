using FluentValidation;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;

namespace SFA.DAS.CommitmentsV2.Validators;

public class ApproveCohortRequestValidator : AbstractValidator<ApproveCohortRequest>
{
    public ApproveCohortRequestValidator()
    {
        RuleFor(r => r.UserInfo).NotNull().SetValidator(new UserInfoValidator());
    }
}