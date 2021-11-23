using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetTransferRequestsSummary
{
    public class GetTransferRequestsSummaryQueryValidator : AbstractValidator<GetTransferRequestsSummaryQuery>
    {
        public GetTransferRequestsSummaryQueryValidator()
        {
            RuleFor(request => request.AccountId).GreaterThan(0);
        }
    }
}
