using System.Threading.Tasks;
using FluentValidation;
using MediatR;

using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;
using Commitment = SFA.DAS.Commitments.Domain.Entities.Commitment;
using LastAction = SFA.DAS.Commitments.Domain.Entities.LastAction;

namespace SFA.DAS.Commitments.Application.Commands.CreateCommitment
{
    public sealed class CreateCommitmentCommandHandler : IAsyncRequestHandler<CreateCommitmentCommand, long>
    {
        private readonly AbstractValidator<CreateCommitmentCommand> _validator;
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly IHashingService _hashingService;
        private readonly ICommitmentsLogger _logger;
        private readonly IHistoryRepository _historyRepository;

        public CreateCommitmentCommandHandler(
            ICommitmentRepository commitmentRepository, 
            IHashingService hashingService, 
            AbstractValidator<CreateCommitmentCommand> validator, 
            ICommitmentsLogger logger, 
            IHistoryRepository historyRepository)
        {
            _commitmentRepository = commitmentRepository;
            _hashingService = hashingService;
            _validator = validator;
            _logger = logger;
            _historyRepository = historyRepository;
        }

        public async Task<long> Handle(CreateCommitmentCommand message)
        {
            _logger.Info($"Employer: {message.Commitment.EmployerAccountId} has called CreateCommitmentCommand", accountId: message.Commitment.EmployerAccountId);

            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var newCommitment = message.Commitment;
            newCommitment.LastAction = LastAction.None;

            newCommitment.Id = await _commitmentRepository.Create(newCommitment);

            await _commitmentRepository.UpdateCommitmentReference(newCommitment.Id, _hashingService.HashValue(newCommitment.Id));

            await CreateMessageIfNeeded(newCommitment.Id, message);

            await CreateHistory(newCommitment, message.Caller.CallerType, message.UserId, message.Commitment.LastUpdatedByEmployerName);

            return newCommitment.Id;
        }

        private async Task CreateHistory(Commitment newCommitment, CallerType callerType, string userId, string userName)
        {
            var historyService = new HistoryService(_historyRepository);
            historyService.TrackInsert(newCommitment, CommitmentChangeType.Created.ToString(), newCommitment.Id, "Commitment", callerType, userId, userName);
            await historyService.Save();
        }

        private async Task CreateMessageIfNeeded(long commitmentId, CreateCommitmentCommand command)
        {
            if (string.IsNullOrEmpty(command.Message))
                return;

            var message = new Message
            {
                Author = command.Commitment.LastUpdatedByEmployerName,
                Text = command.Message,
                CreatedBy = command.Caller.CallerType
            };

            await _commitmentRepository.SaveMessage(commitmentId, message);
        }
    }
}
