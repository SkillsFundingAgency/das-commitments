using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAccountSummary;

public class GetAccountSummaryQueryValidator : AbstractValidator<GetAccountSummaryQuery>
{
    public GetAccountSummaryQueryValidator()
    {
        RuleFor(model => model.AccountId).Must(id => id > 0);
    }
}