using System;
using FluentValidation;
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
            if (string.IsNullOrWhiteSpace(message.Assignee))
                throw new ValidationException("Must specify an assignee");

            var taskTemplate = await _taskRepository.GetTemplateById(message.TaskTemplateId);

            if (taskTemplate == null)
                throw new ValidationException("Unknown task template");

            var newTask = new Domain.Entities.Task
            {
                Assignee = message.Assignee,
                TaskTemplateId = message.TaskTemplateId,
                Name = taskTemplate.Name,
                Body = message.Body,
                TaskStatus = TaskStatuses.Open,
                CreatedOn = DateTime.UtcNow
            };

            await _taskRepository.Create(newTask);
        }
    }
}
