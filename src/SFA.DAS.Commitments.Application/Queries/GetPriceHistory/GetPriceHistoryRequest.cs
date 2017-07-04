using MediatR;

using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.Queries.GetPriceHistory
{
    public sealed class GetPriceHistoryRequest : IAsyncRequest<GetPriceHistoryResponse>
    {
        public Caller Caller { get; set; }

        public long ApprenticeshipId { get; set; }
    }
}