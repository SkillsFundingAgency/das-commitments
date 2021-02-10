using MediatR;

namespace SFA.DAS.Commitments.Application.Queries.GetProvider
{
    public class GetProviderQuery : IAsyncRequest<GetProviderQueryResponse>
    {
        public long Ukprn { get; set; }
    }
}