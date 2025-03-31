using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSupportStatus;

public class GetCohortSupportStatusQueryValidator :  AbstractValidator<GetCohortSupportStatusQuery>
{
    public GetCohortSupportStatusQueryValidator()
    {
        RuleFor(model => model.CohortId).GreaterThan(0).WithMessage("The cohort id must be supplied");
    }
}