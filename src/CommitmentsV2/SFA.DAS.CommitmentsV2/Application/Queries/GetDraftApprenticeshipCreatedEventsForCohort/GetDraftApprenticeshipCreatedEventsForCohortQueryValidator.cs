using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeshipCreatedEventsForCohort;

public class GetDraftApprenticeshipCreatedEventsForCohortQueryValidator :  AbstractValidator<GetDraftApprenticeshipCreatedEventsForCohortQuery>
{
    public GetDraftApprenticeshipCreatedEventsForCohortQueryValidator()
    {
        RuleFor(model => model.CohortId).Must(id => id > 0).WithMessage("The Account Id must be supplied");
        RuleFor(model => model.ProviderId).Must(id => id > 0).WithMessage("The Provider Id must be supplied");
    }
}