using System;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Tasks.Domain.Repositories;

namespace SFA.DAS.Tasks.Application.Queries.GetTasks
{
    public sealed class GetTasksQueryHandler : IAsyncRequestHandler<GetTasksRequest, GetTasksResponse>
    {
        private readonly ITaskRepository _taskRepository;

        public GetTasksQueryHandler(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public async Task<GetTasksResponse> Handle(GetTasksRequest message)
        {
            var tasks = await _taskRepository.GetByAssignee(message.Assignee);

            return new GetTasksResponse {Data = tasks};
        }
    }
}
