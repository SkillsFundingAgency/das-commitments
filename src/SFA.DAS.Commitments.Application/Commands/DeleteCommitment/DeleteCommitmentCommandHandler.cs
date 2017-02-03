using System;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.Commands.DeleteCommitment
{
    public sealed class DeleteCommitmentCommandHandler : AsyncRequestHandler<DeleteCommitmentCommand>
    {
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly AbstractValidator<DeleteCommitmentCommand> _validator;
        private readonly ICommitmentsLogger _logger;

        public DeleteCommitmentCommandHandler(ICommitmentRepository commitmentRepository, AbstractValidator<DeleteCommitmentCommand> validator, ICommitmentsLogger logger)
        {
            _commitmentRepository = commitmentRepository;
            _validator = validator;
            _logger = logger;
        }

        protected override Task HandleCore(DeleteCommitmentCommand command)
        {
            LogMessage(command);

            var validationResult = _validator.Validate(command);

            throw new NotImplementedException();
        }

        private void LogMessage(DeleteCommitmentCommand command)
        {
            string messageTemplate = $"{command.Caller.CallerType}: {command.Caller.Id} has called DeleteCommitmentCommand";

            if (command.Caller.CallerType == CallerType.Employer)
                _logger.Info(messageTemplate, accountId: command.Caller.Id, commitmentId: command.CommitmentId);
            else
                _logger.Info(messageTemplate, providerId: command.Caller.Id, commitmentId: command.CommitmentId);
        }
    }
}
