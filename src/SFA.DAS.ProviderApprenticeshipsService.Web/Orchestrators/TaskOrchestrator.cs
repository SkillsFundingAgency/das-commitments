using System;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.ProviderApprenticeshipsService.Application.Queries.GetTask;
using SFA.DAS.ProviderApprenticeshipsService.Application.Queries.GetTasks;
using SFA.DAS.ProviderApprenticeshipsService.Web.Models;

namespace SFA.DAS.ProviderApprenticeshipsService.Web.Orchestrators
{
    public class TaskOrchestrator
    {
        private readonly IMediator _mediator;

        public TaskOrchestrator(IMediator mediator)
        {
            if (mediator == null)
                throw new ArgumentNullException(nameof(mediator));
            _mediator = mediator;
        }

        public async Task<TaskListViewModel> GetAll(long providerId)
        {
            var response = await _mediator.SendAsync(new GetTasksQueryRequest
            {
                ProviderId = providerId
            });

            return new TaskListViewModel
            {
                ProviderId = providerId,
                Tasks = response.Tasks
            };
        }

        public async Task<TaskViewModel> GetTask(long taskId, long providerId)
        {
            var response = await _mediator.SendAsync(new GetTaskQueryRequest
            {
                ProviderId = providerId,
                TaskId = taskId
            });

            return new TaskViewModel
            {
                Task = response.Task
            };
        }
    }
}