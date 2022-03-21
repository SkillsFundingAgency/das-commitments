using FluentValidation;
using SFA.DAS.Authorization.Services;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;

namespace SFA.DAS.CommitmentsV2.Validators
{
    public class AddDraftApprenticeshipRequestValidator : AbstractValidator<AddDraftApprenticeshipRequest>
    {
        public AddDraftApprenticeshipRequestValidator(IAuthorizationService authorizationService)
        {
            RuleFor(r => r.UserId).NotEmpty().WithMessage("The user id must be supplied"); 
            RuleFor(r => r.ProviderId).Must(p => p > 0).WithMessage("The provider id must be positive");
            RuleFor(r => r.FirstName).MaximumLength(100).WithMessage("You must enter a first name that's no longer than 100 characters");
            RuleFor(r => r.LastName).MaximumLength(100).WithMessage("You must enter a last name that's no longer than 100 characters"); 
            RuleFor(r => r.OriginatorReference).MaximumLength(20).WithMessage("The Reference must be 20 characters or fewer");
            RuleFor(r => r.ReservationId).NotEmpty().WithMessage("The reservation id must be supplied");
            RuleFor(r => r.UserInfo).SetValidator(new UserInfoValidator()).When(r => r.UserInfo != null);
            RuleFor(model => (int)model.DeliveryModel).InclusiveBetween(0, 1).WithMessage("Delivery Model can only be 0 or 1").When(model => model.DeliveryModel.HasValue);
        }
    }
}