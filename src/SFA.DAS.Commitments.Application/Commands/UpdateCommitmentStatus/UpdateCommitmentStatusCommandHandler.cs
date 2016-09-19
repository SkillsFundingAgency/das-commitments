using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Tasks.Api.Client;

namespace SFA.DAS.Commitments.Application.Commands.UpdateCommitmentStatus
{
    public sealed class UpdateCommitmentStatusCommandHandler : AsyncRequestHandler<UpdateCommitmentStatusCommand>
    {
        private readonly AbstractValidator<UpdateCommitmentStatusCommand> _validator;
        private readonly ITasksApi _tasksApi;
        private readonly ICommitmentRepository _commitmentRepository;

        public UpdateCommitmentStatusCommandHandler(ITasksApi tasksApi, ICommitmentRepository commitmentRepository, AbstractValidator<UpdateCommitmentStatusCommand> validator)
        {
            if (tasksApi == null)
                throw new ArgumentNullException(nameof(tasksApi));
            if (commitmentRepository == null)
                throw new ArgumentNullException(nameof(commitmentRepository));
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));
            _tasksApi = tasksApi;
            _commitmentRepository = commitmentRepository;
            _validator = validator;
        }

        protected override async Task HandleCore(UpdateCommitmentStatusCommand message)
        {
            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid)
            {
                throw new InvalidRequestException();
            }

            var commitment = await _commitmentRepository.GetById(message.CommitmentId);

            if (message.Status.HasValue && commitment.Status != (CommitmentStatus) message.Status.Value)
            {
                await _commitmentRepository.UpdateStatus(message.CommitmentId, (CommitmentStatus)message.Status);
            }

            if (!string.IsNullOrWhiteSpace(message.Message))
            {
                var assignee = $"EMPLOYER-{commitment.EmployerAccountId}";

                await _tasksApi.CreateTask(assignee, new Tasks.Domain.Entities.Task
                {
                    Assignee = assignee,
                    Body = message.Message,
                    TaskTemplateId = 1,
                    Name = "SubmitCommitment",
                    CreatedOn = DateTime.UtcNow,
                    TaskStatus = 0
                });
            }
        }
    }
}
