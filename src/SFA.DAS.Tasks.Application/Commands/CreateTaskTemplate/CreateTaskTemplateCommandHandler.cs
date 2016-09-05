using System;
using MediatR;
using SFA.DAS.Tasks.Domain.Entities;
using SFA.DAS.Tasks.Domain.Repositories;
using Task = System.Threading.Tasks.Task;

namespace SFA.DAS.Tasks.Application.Commands.CreateTaskTemplate
{
    public class CreateTaskTemplateCommandHandler : AsyncRequestHandler<CreateTaskTemplateCommand>
    {
        private readonly ITaskRepository _taskRepository;

        public CreateTaskTemplateCommandHandler(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        protected override async Task HandleCore(CreateTaskTemplateCommand message)
        {
            var newTaskTemplate = new TaskTemplate
            {
                Name = message.Name
            };

            await _taskRepository.Create(newTaskTemplate);
        }
    }
}
