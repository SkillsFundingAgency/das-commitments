using System;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Application.Commands.CreateRelationship;
using SFA.DAS.Commitments.Application.Queries.GetRelationship;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;
using Commitment = SFA.DAS.Commitments.Domain.Entities.Commitment;
using CommitmentStatus = SFA.DAS.Commitments.Domain.Entities.CommitmentStatus;
using EditStatus = SFA.DAS.Commitments.Domain.Entities.EditStatus;
using LastAction = SFA.DAS.Commitments.Domain.Entities.LastAction;
using Relationship = SFA.DAS.Commitments.Api.Types.Relationship;

namespace SFA.DAS.Commitments.Application.Commands.CreateCommitment
{
    public sealed class CreateCommitmentCommandHandler : IAsyncRequestHandler<CreateCommitmentCommand, long>
    {
        private readonly AbstractValidator<CreateCommitmentCommand> _validator;
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly IHashingService _hashingService;
        private readonly ICommitmentsLogger _logger;
        private readonly IMediator _mediator;
        private readonly IHistoryRepository _historyRepository;

        public CreateCommitmentCommandHandler(ICommitmentRepository commitmentRepository, IHashingService hashingService, AbstractValidator<CreateCommitmentCommand> validator, ICommitmentsLogger logger, IMediator mediator, IHistoryRepository historyRepository)
        {
            _commitmentRepository = commitmentRepository;
            _hashingService = hashingService;
            _validator = validator;
            _logger = logger;
            _mediator = mediator;
            _historyRepository = historyRepository;
        }

        public async Task<long> Handle(CreateCommitmentCommand message)
        {
            _logger.Info($"Employer: {message.Commitment.EmployerAccountId} has called CreateCommitmentCommand", accountId: message.Commitment.EmployerAccountId);

            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            await CreateRelationshipIfDoesNotAlreadyExist(message);

            var newCommitment = MapFrom(message.Commitment);

            newCommitment.Id = await _commitmentRepository.Create(newCommitment);

            await _commitmentRepository.UpdateCommitmentReference(newCommitment.Id, _hashingService.HashValue(newCommitment.Id));

            await CreateMessageIfNeeded(newCommitment.Id, message);

            await CreateHistory(newCommitment, message.CallerType, message.UserId);

            return newCommitment.Id;
        }

        private async Task CreateHistory(Commitment newCommitment, CallerType callerType, string userId)
        {
            var historyService = new HistoryService(_historyRepository, newCommitment, CommitmentChangeType.Created.ToString(), newCommitment.Id, "Commitment", callerType, userId);
            await historyService.CreateInsert();
        }

        private async Task CreateRelationshipIfDoesNotAlreadyExist(CreateCommitmentCommand message)
        {
            var relationship = await _mediator.SendAsync(new GetRelationshipRequest
            {
                EmployerAccountId = message.Commitment.EmployerAccountId,
                ProviderId = message.Commitment.ProviderId.Value,
                LegalEntityId = message.Commitment.LegalEntityId
            });

            if (relationship.Data == null)
            {
                _logger.Info($"Creating relationship between employer account {message.Commitment.EmployerAccountId}," +
                             $" legal entity {message.Commitment.LegalEntityId}," +
                             $" and provider {message.Commitment.ProviderId}");

                await _mediator.SendAsync(new CreateRelationshipCommand
                {
                    Relationship = new Relationship
                    {
                        EmployerAccountId = message.Commitment.EmployerAccountId,
                        LegalEntityId = message.Commitment.LegalEntityId,
                        LegalEntityName = message.Commitment.LegalEntityName,
                        LegalEntityAddress = message.Commitment.LegalEntityAddress,
                        LegalEntityOrganisationType = message.Commitment.LegalEntityOrganisationType,
                        ProviderId = message.Commitment.ProviderId.Value,
                        ProviderName = message.Commitment.ProviderName
                    }
                });
            }
        }

        private async Task CreateMessageIfNeeded(long commitmentId, CreateCommitmentCommand command)
        {
            if (string.IsNullOrEmpty(command.Message))
                return;

            var message = new Message
            {
                Author = command.Commitment.EmployerLastUpdateInfo.Name,
                Text = command.Message,
                CreatedBy = command.CallerType
            };

            await _commitmentRepository.SaveMessage(commitmentId, message);
        }

        private static Commitment MapFrom(Api.Types.Commitment.Commitment commitment)
        {
            var domainCommitment = new Commitment
            {
                Reference = commitment.Reference,
                EmployerAccountId = commitment.EmployerAccountId,
                LegalEntityId = commitment.LegalEntityId,
                LegalEntityName = commitment.LegalEntityName,
                LegalEntityAddress = commitment.LegalEntityAddress,
                LegalEntityOrganisationType = (OrganisationType) commitment.LegalEntityOrganisationType,
                ProviderId = commitment.ProviderId,
                ProviderName = commitment.ProviderName,
                CommitmentStatus = (CommitmentStatus) commitment.CommitmentStatus,
                EditStatus = (EditStatus) commitment.EditStatus,
                LastAction = LastAction.None,
                LastUpdatedByEmployerName = commitment.EmployerLastUpdateInfo.Name,
                LastUpdatedByEmployerEmail = commitment.EmployerLastUpdateInfo.EmailAddress,
            };

            return domainCommitment;
        }
    }
}
