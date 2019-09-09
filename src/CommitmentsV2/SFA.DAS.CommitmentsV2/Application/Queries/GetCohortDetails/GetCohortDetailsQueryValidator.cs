using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCohortDetails
{
    public class GetCohortDetailsQueryValidator : AbstractValidator<GetCohortDetailsQuery>
    {
        public GetCohortDetailsQueryValidator()
        {
            RuleFor(model => model.CohortId).GreaterThan(0);
        }
    }
}
