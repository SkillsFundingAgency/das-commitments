using System;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using NLog;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.Commands.CreateCommitment
{
    public sealed class CreateCommitmentCommandHandler : IAsyncRequestHandler<CreateCommitmentCommand, long>
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly AbstractValidator<CreateCommitmentCommand> _validator;
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly IHashingService _hashingService;

        public CreateCommitmentCommandHandler(ICommitmentRepository commitmentRepository, IHashingService hashingService, AbstractValidator<CreateCommitmentCommand> validator)
        {
            if (commitmentRepository == null)
                throw new ArgumentNullException(nameof(commitmentRepository));
            if (hashingService == null)
                throw new ArgumentNullException(nameof(hashingService));
            _commitmentRepository = commitmentRepository;
            _hashingService = hashingService;
            _validator = validator;
        }

        public async Task<long> Handle(CreateCommitmentCommand message)
        {
            Logger.Info(BuildInfoMessage(message));

            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var newCommitment = MapFrom(message.Commitment);

            var commitmentId = await _commitmentRepository.Create(newCommitment);

            await _commitmentRepository.UpdateReference(commitmentId, _hashingService.HashValue(commitmentId));

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
                CommitmentStatus = CommitmentStatus.New,
                EditStatus = EditStatus.EmployerOnly,
                Apprenticeships = commitment.Apprenticeships.Select(x => new Apprenticeship
                {
                    Id = x.Id,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    ULN = x.ULN,
                    CommitmentId = commitment.Id,
                    PaymentStatus = (PaymentStatus)x.PaymentStatus,
                    AgreementStatus = (AgreementStatus)x.AgreementStatus,
                    TrainingType = (TrainingType)x.TrainingType,
                    TrainingCode = x.TrainingCode,
                    TrainingName = x.TrainingName,
                    Cost = x.Cost,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate
                }).ToList()
            };

            return domainCommitment;
        }

        private static string BuildInfoMessage(CreateCommitmentCommand cmd)
        {
            return $"Employer: {cmd.Commitment.EmployerAccountId} has called CreateCommitmentCommand";
        }
    }
}
