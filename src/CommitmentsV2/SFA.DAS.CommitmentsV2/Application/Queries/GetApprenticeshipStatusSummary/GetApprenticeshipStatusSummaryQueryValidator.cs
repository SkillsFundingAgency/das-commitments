using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipStatusSummary;

public class GetApprenticeshipStatusSummaryQueryValidator : AbstractValidator<GetApprenticeshipStatusSummaryQuery>
{
    public GetApprenticeshipStatusSummaryQueryValidator()
    {
        RuleFor(x => x.EmployerAccountId).GreaterThan(0);
    }
}