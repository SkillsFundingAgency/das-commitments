using FluentValidation;

namespace SFA.DAS.Commitments.Support.SubSite.Application.Queries.GetSupportCohortSummary
{
    public class GetCohortApprenticeshipsQueryResultValidator : AbstractValidator<GetSupportCohortSummaryQuery>
    {
        public GetCohortApprenticeshipsQueryResultValidator()
        {
            RuleFor(model => model.CohortId).GreaterThan(0);
        }
    }
}