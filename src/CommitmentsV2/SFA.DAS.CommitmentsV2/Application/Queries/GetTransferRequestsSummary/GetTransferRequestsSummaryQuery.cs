using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetTransferRequestsSummary
{
    public class GetTransferRequestsSummaryQuery : IRequest<GetTransferRequestsSummaryQueryResult>
    {
        public long AccountId { get; }
        
        public GetTransferRequestsSummaryQuery(long accountId)
        {
            AccountId = accountId;
        }
    }
}
