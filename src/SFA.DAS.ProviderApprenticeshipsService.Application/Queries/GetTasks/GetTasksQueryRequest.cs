using MediatR;

namespace SFA.DAS.ProviderApprenticeshipsService.Application.Queries.GetTasks
{
    public class GetTasksQueryRequest : IAsyncRequest<GetTasksQueryResponse>
    {
        public long ProviderId { get; set; }
    }
}