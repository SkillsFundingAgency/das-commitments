using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCohortEmailOverlaps;

public class GetCohortEmailOverlapsQueryValidator :  AbstractValidator<GetCohortEmailOverlapsQuery>
{
    public GetCohortEmailOverlapsQueryValidator()
    {
        RuleFor(model => model.CohortId).GreaterThan(0).WithMessage("The cohort id must be supplied");
    }
}