using System;
using System.Linq;
using FluentValidation;
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
            var existingTemplates = await _taskRepository.GetAll();

            if (existingTemplates.Any(a => a.Name.Equals(message.Name, StringComparison.CurrentCultureIgnoreCase)))
                throw new ValidationException("Task template already exists with this name");

            var newTaskTemplate = new TaskTemplate
            {
                Name = message.Name
            };

            await _taskRepository.Create(newTaskTemplate);
        }
    }
}
