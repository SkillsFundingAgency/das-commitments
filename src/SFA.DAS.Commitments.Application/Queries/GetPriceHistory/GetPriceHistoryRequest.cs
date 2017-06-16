using MediatR;

namespace SFA.DAS.Commitments.Application.Queries.GetPriceHistory
{
    public sealed class GetPriceHistoryRequest : IAsyncRequest<GetPriceHistoryResponse>
    {
        public long ApprenticeshipId { get; set; }
    }
}