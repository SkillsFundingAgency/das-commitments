using FluentValidation;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.CanAccessCohort
{
    public class CanAccessCohortValidator :  AbstractValidator<CanAccessCohortQuery>
    {
        public CanAccessCohortValidator()
        {
            RuleFor(model => model.CohortId).GreaterThan(0).WithMessage("The cohort id must be supplied");
            RuleFor(model => model.AccountId).Must(accountId => accountId.HasValue)
                .When(model => model.PartyType == PartyType.Employer).WithMessage("The Account Id must be supplied when the party type is Employer");
            RuleFor(model => model.ProviderId).Must(providerId => providerId.HasValue)
                .When(model => model.PartyType == PartyType.Provider).WithMessage("The Provider Id must be supplied when the party type is Provider");
        }
    }
}