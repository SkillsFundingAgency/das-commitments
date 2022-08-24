using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetOverlapRequests
{
    public class GetOverlapRequestsQueryValidator : AbstractValidator<GetOverlapRequestsQuery>
    {
        public GetOverlapRequestsQueryValidator()
        {
            RuleFor(q => q.DraftApprenticeshipId).GreaterThan(0).WithMessage("The Draft Apprenticeship ID must be supplied");
        }
    }
}