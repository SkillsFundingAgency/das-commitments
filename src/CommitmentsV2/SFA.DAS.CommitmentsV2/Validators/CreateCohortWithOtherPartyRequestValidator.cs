using FluentValidation;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;

namespace SFA.DAS.CommitmentsV2.Validators
{
    public class CreateCohortWithOtherPartyRequestValidator : AbstractValidator<CreateCohortWithOtherPartyRequest>
    {
        public CreateCohortWithOtherPartyRequestValidator()
        {
            RuleFor(r => r.AccountId).Must(accountId => accountId > 0).WithMessage("The Account Id valid");
            RuleFor(r => r.AccountLegalEntityId).Must(accountLegalEntityId => accountLegalEntityId > 0).WithMessage("The Account Legal Entity must be valid");
            RuleFor(r => r.ProviderId).Must(providerId => providerId > 0).WithMessage("The provider id must be valid");
            RuleFor(r => r.Message).Must(message => message == null || message.Length <= 500).WithMessage("The Message cannot be more than 500 characters");
            RuleFor(r => r.UserInfo).SetValidator(new UserInfoValidator()).When(r => r.UserInfo != null);
        }
    }
}
