using System.Threading.Tasks;
using FluentValidation;
using MediatR;

using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Events;
using Commitment = SFA.DAS.Commitments.Domain.Entities.Commitment;
using LastAction = SFA.DAS.Commitments.Domain.Entities.LastAction;
using SFA.DAS.HashingService;
using SFA.DAS.Messaging.Interfaces;

namespace SFA.DAS.Commitments.Application.Commands.CreateCommitment
{
    public sealed class CreateCommitmentCommandHandler : IAsyncRequestHandler<CreateCommitmentCommand, long>
    {
        private readonly AbstractValidator<CreateCommitmentCommand> _validator;
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly IRelationshipRepository _relationshipRepository;
        private readonly IHashingService _hashingService;
        private readonly ICommitmentsLogger _logger;
        private readonly IHistoryRepository _historyRepository;
        private readonly IMessagePublisher _messagePublisher;

        public CreateCommitmentCommandHandler(ICommitmentRepository commitmentRepository,
            IHashingService hashingService,
            AbstractValidator<CreateCommitmentCommand> validator,
            ICommitmentsLogger logger,
            IHistoryRepository historyRepository,
            IMessagePublisher messagePublisher,
            IRelationshipRepository relationshipRepository)
        {
            _commitmentRepository = commitmentRepository;
            _hashingService = hashingService;
            _validator = validator;
            _logger = logger;
            _historyRepository = historyRepository;
            _messagePublisher = messagePublisher;
            _relationshipRepository = relationshipRepository;
        }

        public async Task<long> Handle(CreateCommitmentCommand message)
        {
            _logger.Info($"Employer: {message.Commitment.EmployerAccountId} has called CreateCommitmentCommand", accountId: message.Commitment.EmployerAccountId);

            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            Relationship relationship = null;
            if (await _relationshipRepository.GetRelationship(message.Commitment.EmployerAccountId, message.Commitment.ProviderId.Value, message.Commitment.LegalEntityId) == null)
            {
                relationship = CreateRelationshipFromCommitment(message.Commitment);
            }

            var newCommitment = await CreateCommitment(message, relationship);

            await Task.WhenAll(
                CreateMessageIfNeeded(newCommitment.Id, message),
                CreateHistory(newCommitment, message.Caller.CallerType, message.UserId, message.Commitment.LastUpdatedByEmployerName),
                PublishCohortCreatedEvent(newCommitment),
                PublishRelationshipCreatedEventIfNeeded(relationship)
            );
            
            return newCommitment.Id;
        }

        private async Task PublishCohortCreatedEvent(Commitment newCommitment)
        {
            await _messagePublisher.PublishAsync(new CohortCreated(newCommitment.EmployerAccountId, newCommitment.ProviderId,
                newCommitment.Id));
        }

        private async Task<Commitment> CreateCommitment(CreateCommitmentCommand message, Relationship relationshipToCreate)
        {
            var newCommitment = message.Commitment;
            newCommitment.LastAction = message.LastAction;

            newCommitment.Id = await _commitmentRepository.Create(newCommitment, relationshipToCreate);
            await _commitmentRepository.UpdateCommitmentReference(newCommitment.Id, _hashingService.HashValue(newCommitment.Id));

            return newCommitment;
        }

        private async Task CreateHistory(Commitment newCommitment, CallerType callerType, string userId, string userName)
        {
            var historyService = new HistoryService(_historyRepository);
            historyService.TrackInsert(newCommitment, CommitmentChangeType.Created.ToString(), newCommitment.Id, null, callerType, userId, newCommitment.ProviderId, newCommitment.EmployerAccountId, userName);
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

        private async Task PublishRelationshipCreatedEventIfNeeded(Relationship relationship)
        {
            if (relationship == null) return;
            await _messagePublisher.PublishAsync(CreateRelationshipCreatedEvent(relationship));
        }

        private static Relationship CreateRelationshipFromCommitment(Commitment commitment)
        {
            return new Relationship
            {
                EmployerAccountId = commitment.EmployerAccountId,
                LegalEntityId = commitment.LegalEntityId,
                LegalEntityName = commitment.LegalEntityName,
                LegalEntityAddress = commitment.LegalEntityAddress,
                LegalEntityOrganisationType = commitment.LegalEntityOrganisationType,
                ProviderId = commitment.ProviderId.Value,
                ProviderName = commitment.ProviderName
            };
        }

        private static RelationshipCreated CreateRelationshipCreatedEvent(Relationship entity)
        {
            return new RelationshipCreated(new Api.Types.Relationship
            {
                EmployerAccountId = entity.EmployerAccountId,
                Id = entity.Id,
                LegalEntityId = entity.LegalEntityId,
                LegalEntityName = entity.LegalEntityName,
                LegalEntityAddress = entity.LegalEntityAddress,
                LegalEntityOrganisationType = entity.LegalEntityOrganisationType,
                ProviderId = entity.ProviderId,
                ProviderName = entity.ProviderName,
                Verified = entity.Verified
            });
        }
    }
}
