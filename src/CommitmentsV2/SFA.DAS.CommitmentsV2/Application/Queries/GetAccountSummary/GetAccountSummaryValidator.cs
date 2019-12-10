using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAccountSummary
{
    public class GetAccountSummaryValidator : AbstractValidator<GetAccountSummaryRequest>
    {
        public GetAccountSummaryValidator()
        {
            RuleFor(model => model.AccountId).Must(id => id > 0);
        }
    }
}
