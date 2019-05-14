using FluentValidation;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;

namespace SFA.DAS.CommitmentsV2.Validators
{
    public class AddDraftApprenticeshipRequestValidator : AbstractValidator<AddDraftApprenticeshipRequest>
    {
        public AddDraftApprenticeshipRequestValidator()
        {
            RuleFor(r => r.CohortId).Must(i => i > 0).WithMessage("The cohort id must be supplied");
            RuleFor(r => r.UserId).NotEmpty().WithMessage("The user id must be supplied");
            RuleFor(r => r.AccountLegalEntityId).Must(i => i > 0).WithMessage("The Account Legal Entity must be valid"); 
            RuleFor(r => r.ProviderId).Must(i => i > 0).WithMessage("The provider id must be positive");
            RuleFor(r => r.ReservationId).NotEmpty().WithMessage("The reservation id must be supplied");
            RuleFor(r => r.FirstName).MaximumLength(100).WithMessage("You must enter a first name that's no longer than 100 characters");
            RuleFor(r => r.LastName).MaximumLength(100).WithMessage("You must enter a last name that's no longer than 100 characters"); 
            RuleFor(r => r.OriginatorReference).MaximumLength(20).WithMessage("The Reference must be 20 characters or fewer");
        }
    }
}