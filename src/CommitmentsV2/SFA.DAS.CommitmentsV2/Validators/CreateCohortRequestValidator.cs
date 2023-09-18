using FluentValidation;
using SFA.DAS.Authorization.Services;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;

namespace SFA.DAS.CommitmentsV2.Validators
{
    public class CreateCohortRequestValidator : AbstractValidator<CreateCohortRequest>
    {
        public CreateCohortRequestValidator()
        {
            RuleFor(r => r.AccountId).Must(accountId => accountId > 0).WithMessage("The Account Id valid");
            RuleFor(r => r.AccountLegalEntityId).Must(accountLegalEntityId => accountLegalEntityId > 0).WithMessage("The Account Legal Entity must be valid"); 
            RuleFor(r => r.ProviderId).Must(providerId => providerId > 0).WithMessage("The provider id must be positive");
            RuleFor(r => r.FirstName).MaximumLength(100).WithMessage("You must enter a first name that's no longer than 100 characters");
            RuleFor(r => r.LastName).MaximumLength(100).WithMessage("You must enter a last name that's no longer than 100 characters"); 
            RuleFor(r => r.OriginatorReference).MaximumLength(20).WithMessage("The Reference must be 20 characters or fewer");
            RuleFor(r => r.ReservationId).NotEmpty().WithMessage("The reservation id must be supplied");
            RuleFor(r => r.UserInfo).SetValidator(new UserInfoValidator()).When(r => r.UserInfo != null);
            RuleFor(model => (int)model.DeliveryModel).InclusiveBetween(0, 2).WithMessage("Delivery Model can only be between 0 and 2").When(model => model.DeliveryModel.HasValue);
            RuleFor(r => r.IsOnFlexiPaymentPilot).NotNull().WithMessage("Select whether this apprentice will be on the pilot programme.");
        }
    }
}
