using FluentValidation;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAccountLegalEntity;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary
{
    public class GetCohortSummaryValidator :  AbstractValidator<GetCohortSummaryRequest>
    {
        public GetCohortSummaryValidator()
        {
            RuleFor(model => model.CohortId).GreaterThan(0).WithMessage("The cohort id must be supplied");
        }
    }
}
