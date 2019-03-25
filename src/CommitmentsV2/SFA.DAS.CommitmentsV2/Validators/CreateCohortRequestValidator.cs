using FluentValidation;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;

namespace SFA.DAS.CommitmentsV2.Validators
{
    public class CreateCohortRequestValidator : AbstractValidator<CreateCohortRequest>
    {
        public CreateCohortRequestValidator()
        {
            RuleFor(model => model).SetValidator(new NameValidator());
            RuleFor(model => model).SetValidator(new UlnValidator());
            RuleFor(model => model.UserId).NotEmpty().WithMessage("The user id must be supplied");
            RuleFor(model => model.AccountLegalEntityId).Must(accountLegalEntityId => accountLegalEntityId > 0).WithMessage("The Account Legal Entity must be valid"); 
            RuleFor(model => model.ProviderId).Must(providerId => providerId > 0).WithMessage("The provider id must be positive");
            //RuleFor(model => model.EndDate).Must((request, endDate) => endDate > request.StartDate).When(request => request.EndDate.HasValue && request.StartDate.HasValue).WithMessage("The end date must be later than the start date");
            RuleFor(model => model.ReservationId).NotEmpty().WithMessage("The reservation id must be supplied");
        }
    }
}
