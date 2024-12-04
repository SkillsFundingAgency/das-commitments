using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetPendingOverlapRequests;

public class GetPendingOverlapRequestsQueryValidator : AbstractValidator<GetPendingOverlapRequestsQuery>
{
    public GetPendingOverlapRequestsQueryValidator()
    {
        RuleFor(q => q.DraftApprenticeshipId).GreaterThan(0).WithMessage("The Draft Apprenticeship ID must be supplied");
    }
}