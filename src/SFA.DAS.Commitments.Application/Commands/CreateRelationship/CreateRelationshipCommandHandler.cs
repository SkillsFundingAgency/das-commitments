using System;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Events;
using SFA.DAS.Messaging.Interfaces;

namespace SFA.DAS.Commitments.Application.Commands.CreateRelationship
{
    public class CreateRelationshipCommandHandler : AsyncRequestHandler<CreateRelationshipCommand>
    {
        private readonly AbstractValidator<CreateRelationshipCommand> _validator;
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly ICommitmentsLogger _logger;
        private readonly IMessagePublisher _messagePublisher;

        public CreateRelationshipCommandHandler(ICommitmentRepository commitmentRepository, AbstractValidator<CreateRelationshipCommand> validator, ICommitmentsLogger logger, IMessagePublisher messagePublisher)
        {
            if (commitmentRepository == null)
                throw new ArgumentNullException(nameof(commitmentRepository));
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _commitmentRepository = commitmentRepository;
            _validator = validator;
            _logger = logger;
            _messagePublisher = messagePublisher;
        }

        protected override async Task HandleCore(CreateRelationshipCommand message)
        {
            _logger.Info($"Employer: {message.Relationship.EmployerAccountId} has called CreateRelationshipCommand", accountId: message.Relationship.EmployerAccountId);

            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            await Task.WhenAll(_commitmentRepository.CreateRelationship(message.Relationship),
                PublishRelationshipCreatedEvent(message.Relationship));
        }

        private async Task PublishRelationshipCreatedEvent(Domain.Entities.Relationship relationship)
        {
            await _messagePublisher.PublishAsync(new RelationshipCreated(Map(relationship)));
        }

        private static Relationship Map(Domain.Entities.Relationship entity)
        {
            return new Relationship
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
            };
        }
    }
}
