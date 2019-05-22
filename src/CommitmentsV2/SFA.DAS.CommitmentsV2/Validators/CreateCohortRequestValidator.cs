using FluentValidation;
using SFA.DAS.Authorization;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Features;

namespace SFA.DAS.CommitmentsV2.Validators
{
    public class CreateCohortRequestValidator : AbstractValidator<CreateCohortRequest>
    {
        public CreateCohortRequestValidator(IAuthorizationService authorizationService)
        {
            RuleFor(r => r.UserId).NotEmpty().WithMessage("The user id must be supplied");
            RuleFor(r => r.AccountLegalEntityId).Must(accountLegalEntityId => accountLegalEntityId > 0).WithMessage("The Account Legal Entity must be valid"); 
            RuleFor(r => r.ProviderId).Must(providerId => providerId > 0).WithMessage("The provider id must be positive");
            RuleFor(r => r.FirstName).MaximumLength(100).WithMessage("You must enter a first name that's no longer than 100 characters");
            RuleFor(r => r.LastName).MaximumLength(100).WithMessage("You must enter a last name that's no longer than 100 characters"); 
            RuleFor(r => r.OriginatorReference).MaximumLength(20).WithMessage("The Reference must be 20 characters or fewer");

            if (authorizationService.IsAuthorized(Feature.Reservations))
            {
                RuleFor(r => r.ReservationId).NotEmpty().WithMessage("The reservation id must be supplied");
            }
            else
            {
                RuleFor(r => r.ReservationId).Null().WithMessage("The reservation id must not be supplied");
            }
        }
    }
}
