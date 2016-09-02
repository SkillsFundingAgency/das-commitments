using System;
using MediatR;
using SFA.DAS.Tasks.Domain.Entities;
using SFA.DAS.Tasks.Domain.Repositories;
using Task = System.Threading.Tasks.Task;

namespace SFA.DAS.Tasks.Application.Commands.CreateTask
{
    public class CreateTaskCommandHandler : AsyncRequestHandler<CreateTaskCommand>
    {
        private readonly ITaskRepository _taskRepository;

        public CreateTaskCommandHandler(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        protected override async Task HandleCore(CreateTaskCommand message)
        {
            //todo: read template, create task based on template attributes
            var newTask = new Domain.Entities.Task
            {
                Assignee = message.Assignee,
                TaskTemplateId = message.TaskTemplateId,
                Name = "New task",
                TaskStatus = TaskStatuses.Open,
                CreatedOn = DateTime.UtcNow
            };

            await _taskRepository.Create(newTask);
        }
    }
}
