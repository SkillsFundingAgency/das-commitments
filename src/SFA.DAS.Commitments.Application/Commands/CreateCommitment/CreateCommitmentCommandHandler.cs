using System;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using NLog;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.Commands.CreateCommitment
{
    public sealed class CreateCommitmentCommandHandler : IAsyncRequestHandler<CreateCommitmentCommand, long>
    {
        private readonly AbstractValidator<CreateCommitmentCommand> _validator;
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly IHashingService _hashingService;
        private readonly ILog _logger;

        public CreateCommitmentCommandHandler(ICommitmentRepository commitmentRepository, IHashingService hashingService, AbstractValidator<CreateCommitmentCommand> validator, ILog logger)
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

        public async Task<long> Handle(CreateCommitmentCommand message)
        {
            _logger.Info($"Employer: {message.Commitment.EmployerAccountId} has called CreateCommitmentCommand");

            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var newCommitment = MapFrom(message.Commitment);

            var commitmentId = await _commitmentRepository.Create(newCommitment);

            await _commitmentRepository.UpdateCommitmentReference(commitmentId, _hashingService.HashValue(commitmentId));

            return commitmentId;
        }

        private static Commitment MapFrom(Api.Types.Commitment commitment)
        {
            var domainCommitment = new Commitment
            {
                Id = commitment.Id,
                Reference = commitment.Reference,
                EmployerAccountId = commitment.EmployerAccountId,
                LegalEntityId = commitment.LegalEntityId,
                LegalEntityName = commitment.LegalEntityName,
                ProviderId = commitment.ProviderId,
                ProviderName = commitment.ProviderName,
                CommitmentStatus = (CommitmentStatus) commitment.CommitmentStatus,
                EditStatus = (EditStatus) commitment.EditStatus,
                LastAction = LastAction.None,
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
