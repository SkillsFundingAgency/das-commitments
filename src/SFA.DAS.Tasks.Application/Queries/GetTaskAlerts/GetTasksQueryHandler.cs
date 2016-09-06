using System;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Tasks.Domain.Repositories;

namespace SFA.DAS.Tasks.Application.Queries.GetTaskAlerts
{
    public sealed class GetTaskAlertsQueryHandler : IAsyncRequestHandler<GetTaskAlertsRequest, GetTaskAlertsResponse>
    {
        private readonly ITaskRepository _taskRepository;

        public GetTaskAlertsQueryHandler(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public async Task<GetTaskAlertsResponse> Handle(GetTaskAlertsRequest message)
        {
            var taskAlerts = await _taskRepository.GetByUser(message.UserId);

            return new GetTaskAlertsResponse {Data = taskAlerts};
        }
    }
}
