using MediatR;

namespace SFA.DAS.ProviderApprenticeshipsService.Application.Queries.GetTask
{
    public class GetTaskQueryRequest : IAsyncRequest<GetTaskQueryResponse>
    {
        public long ProviderId { get; set; }
        public long TaskId { get; set; }
    }
}