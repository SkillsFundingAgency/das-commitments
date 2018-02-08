using System;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
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
                PublishRelationshipCreatedEvent(message.Relationship.ProviderId, message.Relationship.EmployerAccountId,
                    message.Relationship.LegalEntityId));
        }

        private async Task PublishRelationshipCreatedEvent(long providerId, long employerAccountId, string legalEntityId)
        {
            await _messagePublisher.PublishAsync(new RelationshipEvent(providerId, employerAccountId,
                legalEntityId));
        }
    }
}
