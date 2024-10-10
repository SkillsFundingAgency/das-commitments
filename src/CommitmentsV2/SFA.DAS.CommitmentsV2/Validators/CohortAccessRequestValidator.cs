using FluentValidation;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Validators;

public class CohortAccessRequestValidator : AbstractValidator<CohortAccessRequest>
{
    public CohortAccessRequestValidator()
    {
        RuleFor(r => r.CohortId).GreaterThan(0).WithMessage("The Cohort Id must be valid");
        RuleFor(r => r.PartyId).GreaterThan(0).WithMessage("The Party Id must be valid");
        RuleFor(r => r.Party).Must(x=>x == Party.Employer || x == Party.Provider).WithMessage("The Party Type must be Provider or Employer");
    }
}