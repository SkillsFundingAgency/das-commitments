using MediatR;

namespace SFA.DAS.ProviderApprenticeshipsService.Application.Queries.GetCommitments
{
    public class GetCommitmentsQueryRequest : IAsyncRequest<GetCommitmentsQueryResponse>
    {
        public long ProviderId { get; set; }
    }
}