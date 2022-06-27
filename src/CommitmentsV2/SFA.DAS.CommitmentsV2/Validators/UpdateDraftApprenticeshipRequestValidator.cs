using FluentValidation;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Domain;

namespace SFA.DAS.CommitmentsV2.Validators
{
    public class UpdateDraftApprenticeshipRequestValidator :  AbstractValidator<UpdateDraftApprenticeshipRequest>
    {
        public UpdateDraftApprenticeshipRequestValidator()
        {
            RuleFor(model => model.Cost).GreaterThan(ctx => 0).WithMessage("The cost must be greater than zero");
            RuleFor(model => model.CourseCode).MaximumLength(Constants.FieldLengths.CourseCode).WithMessage("Course Code must not be more than {MaxLength} characters");
            RuleFor(model => model.FirstName).MaximumLength(Constants.FieldLengths.FirstName).WithMessage("First Name must not be more than {MaxLength} characters");
            RuleFor(model => model.LastName).MaximumLength(Constants.FieldLengths.LastName).WithMessage("Last Name must not be more than {MaxLength} characters");
            RuleFor(model => model.Reference).MaximumLength(Constants.FieldLengths.ProviderReference).WithMessage("Reference must not be more than {MaxLength} characters");
            RuleFor(model => model.Uln).MaximumLength(Constants.FieldLengths.Uln).WithMessage("ULN must not be more than {MaxLength} characters");
            RuleFor(r => r.UserInfo).SetValidator(new UserInfoValidator()).When(r => r.UserInfo != null);
            RuleFor(model => (int)model.DeliveryModel).InclusiveBetween(0, 2).WithMessage("Delivery Model can only be between 0 and 2");
        }
    }
}
