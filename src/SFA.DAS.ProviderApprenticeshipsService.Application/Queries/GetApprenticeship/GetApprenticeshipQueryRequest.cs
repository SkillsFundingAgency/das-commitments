using MediatR;

namespace SFA.DAS.ProviderApprenticeshipsService.Application.Queries.GetApprenticeship
{
    public class GetApprenticeshipQueryRequest : IAsyncRequest<GetApprenticeshipQueryResponse>
    {
        public long ProviderId { get; set; }
        public long CommitmentId { get; set; }
        public long AppenticeshipId { get; set; }
    }
}