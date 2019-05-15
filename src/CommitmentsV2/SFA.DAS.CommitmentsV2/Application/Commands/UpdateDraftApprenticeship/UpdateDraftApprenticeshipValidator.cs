using FluentValidation;
using SFA.DAS.CommitmentsV2.Domain;

namespace SFA.DAS.CommitmentsV2.Application.Commands.UpdateDraftApprenticeship
{
    public class UpdateDraftApprenticeshipValidator :  AbstractValidator<UpdateDraftApprenticeshipCommand>
    {
        public UpdateDraftApprenticeshipValidator()
        {
            RuleFor(model => model.ApprenticeshipId).GreaterThan(ctx => 0).WithMessage("The Apprenticeship Id must be positive");
            RuleFor(model => model.Cost).GreaterThanOrEqualTo(ctx => 0).WithMessage("The cost must be zero or positive");
            RuleFor(model => model.CourseCode).MaximumLength(Constants.FieldLengths.CourseCode).WithMessage("Course Code must not be more than {MaxLength} characters");
            RuleFor(model => model.FirstName).MaximumLength(Constants.FieldLengths.FirstName).WithMessage("First Name must not be more than {MaxLength} characters");
            RuleFor(model => model.LastName).MaximumLength(Constants.FieldLengths.LastName).WithMessage("Last Name must not be more than {MaxLength} characters");
            RuleFor(model => model.ProviderReference).MaximumLength(Constants.FieldLengths.ProviderReference).WithMessage("Reference must not be more than {MaxLength} characters");
            RuleFor(model => model.Uln).MaximumLength(Constants.FieldLengths.Uln).WithMessage("ULN must not be more than {MaxLength} characters");
        }
    }
}
