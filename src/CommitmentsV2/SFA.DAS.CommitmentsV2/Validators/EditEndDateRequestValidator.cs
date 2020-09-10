using FluentValidation;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;

namespace SFA.DAS.CommitmentsV2.Validators
{
    public class EditEndDateRequestValidator : AbstractValidator<EditEndDateRequest>
    {
        public EditEndDateRequestValidator()
        {
            RuleFor(r => r.ApprenticeshipId).Must(apprenticeshipId => apprenticeshipId > 0).WithMessage("The apprenticeship Id invalid");
            RuleFor(r => r.EndDate).Must(endDate => endDate != null ).WithMessage("The end date must have a value");
            RuleFor(r => r.UserInfo).SetValidator(new UserInfoValidator()).When(r => r.UserInfo != null);
        }
    }
}
