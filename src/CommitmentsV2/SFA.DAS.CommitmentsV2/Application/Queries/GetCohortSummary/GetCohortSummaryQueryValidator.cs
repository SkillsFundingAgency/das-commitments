using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;

public class GetCohortSummaryQueryValidator :  AbstractValidator<GetCohortSummaryQuery>
{
    public GetCohortSummaryQueryValidator()
    {
        RuleFor(model => model.CohortId).GreaterThan(0).WithMessage("The cohort id must be supplied");
    }
}