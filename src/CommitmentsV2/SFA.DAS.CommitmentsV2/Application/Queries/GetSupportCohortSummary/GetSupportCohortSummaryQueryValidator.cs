using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCohortApprenticeships
{
    public class GetCohortApprenticeshipsQueryResultValidator : AbstractValidator<GetSupportCohortSummaryQuery>
    {
        public GetCohortApprenticeshipsQueryResultValidator()
        {
            RuleFor(model => model.CohortId).GreaterThan(0);
        }
    }
}