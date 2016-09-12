using System;
using MediatR;
using SFA.DAS.Tasks.Api.Client;
using SFA.DAS.Tasks.Domain.Entities;
using Task = System.Threading.Tasks.Task;

namespace SFA.DAS.ProviderApprenticeshipsService.Application.Commands.CompleteTask
{
    public class CompleteTaskCommandHandler : AsyncRequestHandler<CompleteTaskCommand>
    {
        private readonly ITasksApi _tasksApi;
        private readonly CompleteTaskCommandValidator _validator;

        public CompleteTaskCommandHandler(ITasksApi tasksApi)
        {
            if (tasksApi == null)
                throw new ArgumentNullException(nameof(tasksApi));
            _tasksApi = tasksApi;
            _validator = new CompleteTaskCommandValidator();
        }

        protected override async Task HandleCore(CompleteTaskCommand message)
        {
            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid)
                throw new InvalidRequestException(validationResult.Errors);

            var assignee = $"PROVIDER-{message.ProviderId}";

            var task = await _tasksApi.GetTask(message.TaskId, assignee);

            task.TaskStatus = TaskStatuses.Complete;

            await _tasksApi.UpdateTask(message.TaskId, task);
        }
    }
}