using System;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Application.Commands.CreateRelationship;
using SFA.DAS.Commitments.Application.Queries.GetRelationship;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using AgreementStatus = SFA.DAS.Commitments.Domain.Entities.AgreementStatus;
using Apprenticeship = SFA.DAS.Commitments.Domain.Entities.Apprenticeship;
using Commitment = SFA.DAS.Commitments.Domain.Entities.Commitment;
using CommitmentStatus = SFA.DAS.Commitments.Domain.Entities.CommitmentStatus;
using EditStatus = SFA.DAS.Commitments.Domain.Entities.EditStatus;
using LastAction = SFA.DAS.Commitments.Domain.Entities.LastAction;
using PaymentStatus = SFA.DAS.Commitments.Domain.Entities.PaymentStatus;
using Relationship = SFA.DAS.Commitments.Api.Types.Relationship;
using TrainingType = SFA.DAS.Commitments.Domain.Entities.TrainingType;

namespace SFA.DAS.Commitments.Application.Commands.CreateCommitment
{
    public sealed class CreateCommitmentCommandHandler : IAsyncRequestHandler<CreateCommitmentCommand, long>
    {
        private readonly AbstractValidator<CreateCommitmentCommand> _validator;
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly IHashingService _hashingService;
        private readonly ICommitmentsLogger _logger;
        private readonly IMediator _mediator;

        public CreateCommitmentCommandHandler(ICommitmentRepository commitmentRepository, IHashingService hashingService, AbstractValidator<CreateCommitmentCommand> validator, ICommitmentsLogger logger, IMediator mediator)
        {
            if (commitmentRepository == null)
                throw new ArgumentNullException(nameof(commitmentRepository));
            if (hashingService == null)
                throw new ArgumentNullException(nameof(hashingService));
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if(mediator==null)
                throw new ArgumentException(nameof(mediator));

            _commitmentRepository = commitmentRepository;
            _hashingService = hashingService;
            _validator = validator;
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<long> Handle(CreateCommitmentCommand message)
        {
            _logger.Info($"Employer: {message.Commitment.EmployerAccountId} has called CreateCommitmentCommand", accountId: message.Commitment.EmployerAccountId);

            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

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

            var newCommitment = MapFrom(message.Commitment);

            var commitmentId = await _commitmentRepository.Create(newCommitment, message.CallerType, message.UserId);

            await _commitmentRepository.UpdateCommitmentReference(commitmentId, _hashingService.HashValue(commitmentId));

            return commitmentId;
        }

        private static Commitment MapFrom(Api.Types.Commitment.Commitment commitment)
        {
            var domainCommitment = new Commitment
            {
                Id = commitment.Id,
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
                Apprenticeships = commitment.Apprenticeships.Select(x => new Apprenticeship
                {
                    Id = x.Id,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    ULN = x.ULN,
                    CommitmentId = commitment.Id,
                    PaymentStatus = (PaymentStatus) x.PaymentStatus,
                    AgreementStatus = (AgreementStatus) x.AgreementStatus,
                    TrainingType = (TrainingType) x.TrainingType,
                    TrainingCode = x.TrainingCode,
                    TrainingName = x.TrainingName,
                    Cost = x.Cost,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate
                }).ToList()
            };

            return domainCommitment;
        }
    }
}
