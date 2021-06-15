using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetDataLockSummaries
{
    public class GetDataLockSummariesQueryValidator : AbstractValidator<GetDataLockSummariesQuery>
    {
        public GetDataLockSummariesQueryValidator()
        {
            RuleFor(q => q.ApprenticeshipId).GreaterThan(0);
        }
    }
}
