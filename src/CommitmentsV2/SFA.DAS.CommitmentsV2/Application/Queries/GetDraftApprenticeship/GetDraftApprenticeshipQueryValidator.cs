using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeship;

public class GetDraftApprenticeshipQueryValidator :  AbstractValidator<GetDraftApprenticeshipQuery>
{
    public GetDraftApprenticeshipQueryValidator()
    {
        RuleFor(model => model.CohortId).GreaterThan(0).WithMessage("The cohort id must be supplied");
        RuleFor(model => model.DraftApprenticeshipId).GreaterThan(0).WithMessage("The draft apprenticeship id must be supplied");
    }
}