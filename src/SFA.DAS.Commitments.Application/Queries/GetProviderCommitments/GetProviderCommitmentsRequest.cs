using MediatR;

namespace SFA.DAS.Commitments.Application.Queries.GetProviderCommitments
{
    public sealed class GetProviderCommitmentsRequest : IAsyncRequest<GetProviderCommitmentsResponse>
    {
        public long ProviderId { get; set; }
    }
}
