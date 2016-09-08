using System;
using System.Linq;
using FluentValidation;
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
            if (string.IsNullOrWhiteSpace(message.UserId))
                throw new ValidationException("Must specify a user");

            var task = await _taskRepository.GetById(message.TaskId);

            if (task == null)
                throw new ValidationException("Unknown task");

            var existingAlerts = await _taskRepository.GetByUser(message.UserId);

            if (existingAlerts.Any(a => a.TaskId == message.TaskId))
                throw new ValidationException("Task alert already exists for this user");

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
