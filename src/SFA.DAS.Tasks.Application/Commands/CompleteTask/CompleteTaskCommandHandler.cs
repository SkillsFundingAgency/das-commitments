using System;
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
            //todo: validate status
            var task = await _taskRepository.GetById(message.TaskId);

            task.TaskStatus = TaskStatuses.Complete;
            task.CompletedOn = DateTime.UtcNow;
            task.CompletedBy = message.CompletedBy;

            await _taskRepository.SetCompleted(task);
        }
    }
}
