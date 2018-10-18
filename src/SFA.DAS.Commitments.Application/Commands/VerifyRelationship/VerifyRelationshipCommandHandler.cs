using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Events;
using SFA.DAS.Messaging.Interfaces;

namespace SFA.DAS.Commitments.Application.Commands.VerifyRelationship
{
    public sealed class VerifyRelationshipCommandHandler: AsyncRequestHandler<VerifyRelationshipCommand>
    {
        private readonly IRelationshipRepository _relationshipRepository;
        private readonly VerifyRelationshipValidator _validator;
        private readonly ICommitmentsLogger _logger;
        private readonly IMessagePublisher _messagePublisher;

        public VerifyRelationshipCommandHandler(IRelationshipRepository relationshipRepository, VerifyRelationshipValidator validator, ICommitmentsLogger logger, IMessagePublisher messagePublisher)
        {
            _relationshipRepository = relationshipRepository;
            _validator = validator;
            _logger = logger;
            _messagePublisher = messagePublisher;
        }

        protected override async Task HandleCore(VerifyRelationshipCommand message)
        {
            _logger.Info($"Provider {message.ProviderId} has called VerifyRelationshipCommand for Employer {message.EmployerAccountId}, LegalEntity {message.LegalEntityId}");

            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            await Task.WhenAll(_relationshipRepository.VerifyRelationship(message.EmployerAccountId, message.ProviderId,
                message.LegalEntityId, message.Verified.Value),
                PublishRelationshipVerifiedEvent(message.ProviderId, message.EmployerAccountId, message.LegalEntityId, message.Verified.Value));
        }

        private async Task PublishRelationshipVerifiedEvent(long providerId, long employerAccountId, string legalEntityId, bool? verified)
        {
            await _messagePublisher.PublishAsync(new RelationshipVerified(providerId, employerAccountId, legalEntityId, verified));
        }
    }
}
