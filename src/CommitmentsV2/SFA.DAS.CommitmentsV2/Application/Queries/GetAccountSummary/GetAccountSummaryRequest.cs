using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAccountSummary
{
    public class GetAccountSummaryRequest : IRequest<GetAccountSummaryResponse>
    {
        public long AccountId { get; set; }
    }
}
