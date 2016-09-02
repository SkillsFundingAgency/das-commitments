using MediatR;

namespace SFA.DAS.Commitments.Application.Queries.GetProviderApprenticeship
{
    public class GetProviderApprenticeshipQueryRequest : IAsyncRequest<GetProviderApprenticeshipQueryResponse>
    {
        public long ProviderId { get; set; }
        public long CommitmentId { get; set; }
        public long ApprenticeshipId { get; set; }  
    }
}