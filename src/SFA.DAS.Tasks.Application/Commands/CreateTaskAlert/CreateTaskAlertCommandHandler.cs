using System;
using MediatR;
using SFA.DAS.Tasks.Domain.Entities;
using SFA.DAS.Tasks.Domain.Repositories;
using Task = System.Threading.Tasks.Task;

namespace SFA.DAS.Tasks.Application.Commands.CreateTaskAlert
{
    public class CreateTaskAlertCommandHandler : AsyncRequestHandler<CreateTaskAlertCommand>
    {
        private readonly ITaskRepository _taskRepository;

        public CreateTaskAlertCommandHandler(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        protected override async Task HandleCore(CreateTaskAlertCommand message)
        {
            //todo: validate uniqueness
            var newTaskAlert = new TaskAlert
            {
                TaskId = message.TaskId,
                UserId = message.UserId,
                CreatedOn = DateTime.UtcNow
            };

            await _taskRepository.Create(newTaskAlert);
        }
    }
}
