using System;
using FluentValidation;
using MediatR;
using SFA.DAS.Tasks.Domain.Entities;
using SFA.DAS.Tasks.Domain.Repositories;
using Task = System.Threading.Tasks.Task;

namespace SFA.DAS.Tasks.Application.Commands.CompleteTask
{
    public class CompleteTaskCommandHandler : AsyncRequestHandler<CompleteTaskCommand>
    {
        private readonly ITaskRepository _taskRepository;

        public CompleteTaskCommandHandler(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        protected override async Task HandleCore(CompleteTaskCommand message)
        {
            if (string.IsNullOrWhiteSpace(message.CompletedBy))
                throw new ValidationException("Must specify a user for 'completedBy'");

            var task = await _taskRepository.GetById(message.TaskId);

            if (task == null)
                throw new ValidationException("Unknown task");

            if (task.TaskStatus != TaskStatuses.Open)
                throw new ValidationException("Task is already completed");

            task.TaskStatus = TaskStatuses.Complete;
            task.CompletedOn = DateTime.UtcNow;
            task.CompletedBy = message.CompletedBy;

            await _taskRepository.SetCompleted(task);
        }
    }
}
