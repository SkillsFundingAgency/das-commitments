using System;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.Commands.CreateRelationship
{
    public class CreateRelationshipCommandHandler : AsyncRequestHandler<CreateRelationshipCommand>
    {
        private readonly AbstractValidator<CreateRelationshipCommand> _validator;
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly IHashingService _hashingService;
        private readonly ICommitmentsLogger _logger;

        public CreateRelationshipCommandHandler(ICommitmentRepository commitmentRepository, IHashingService hashingService, AbstractValidator<CreateRelationshipCommand> validator, ICommitmentsLogger logger)
        {
            if (commitmentRepository == null)
                throw new ArgumentNullException(nameof(commitmentRepository));
            if (hashingService == null)
                throw new ArgumentNullException(nameof(hashingService));
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _commitmentRepository = commitmentRepository;
            _hashingService = hashingService;
            _validator = validator;
            _logger = logger;
        }

        protected override async Task HandleCore(CreateRelationshipCommand message)
        {
            _logger.Info($"Employer: {message.Relationship.EmployerAccountId} has called CreateCommitmentCommand", accountId: message.Relationship.EmployerAccountId);

            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var newRelationship = MapFrom(message.Relationship);

            await _commitmentRepository.CreateRelationship(newRelationship);
        }

        private static Relationship MapFrom(Api.Types.Relationship source)
        {
            return new Relationship
            {
                Id = source.Id,
                EmployerAccountId  = source.EmployerAccountId,
                LegalEntityId  = source.LegalEntityId,
                LegalEntityName  = source.LegalEntityName,
                ProviderId = source.ProviderId,
                ProviderName  = source.ProviderName,
                Verified = source.Verified
            };
        }
    }
}
