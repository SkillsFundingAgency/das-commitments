using FluentValidation;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.Learners.Validators;

namespace SFA.DAS.CommitmentsV2.Validators
{
    public class UlnValidator :AbstractValidator<IUln>
    {
        public UlnValidator()
        {
            RuleFor(model => model.Uln).Must(UlnValidation).WithMessage(x => $"{FailedValidationMessage(x.Uln)}");
        }

        private bool UlnValidation(string uln)
        {
            var validator = new Learners.Validators.UlnValidator();

            return (validator.Validate(uln) == UlnValidationResult.Success);
        }
        private string FailedValidationMessage(string uln)
        {
            var validator = new Learners.Validators.UlnValidator();
            switch (validator.Validate(uln))
            {
                case UlnValidationResult.IsEmptyUlnNumber:
                    return "You must enter a Uln that is not empty";
                case UlnValidationResult.IsInValidTenDigitUlnNumber:
                    return "You must enter a Uln that is 10 digits long";
                case UlnValidationResult.IsInvalidUln:
                    return "You must enter a valid Uln";
            }

            return "";
        }
    }
}