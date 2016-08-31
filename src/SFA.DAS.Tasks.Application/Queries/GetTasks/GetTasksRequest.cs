using System;
using MediatR;

namespace SFA.DAS.Tasks.Application.Queries.GetTasks
{
    public sealed class GetTasksRequest : IAsyncRequest<GetTasksResponse>
    {
        public string Assignee { get; set; }
    }
}
