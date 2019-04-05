using FluentValidation;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;

namespace SFA.DAS.CommitmentsV2.Validators
{
    public class NameValidator : AbstractValidator<IName>
    {
        public NameValidator()
        {
            RuleFor(model => model.FirstName).NotEmpty().WithMessage("First name must be entered").MaximumLength(100).WithMessage("You must enter a first name that's no longer than 100 characters");
            RuleFor(model => model.LastName).NotEmpty().WithMessage("Last name must be entered").MaximumLength(100).WithMessage("You must enter a last name that's no longer than 100 characters");
        }
    }
}
