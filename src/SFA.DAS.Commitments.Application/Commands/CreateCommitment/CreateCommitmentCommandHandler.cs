using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using NLog;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;

namespace SFA.DAS.Commitments.Application.Commands.CreateCommitment
{
    public sealed class CreateCommitmentCommandHandler : IAsyncRequestHandler<CreateCommitmentCommand, long>
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly AbstractValidator<CreateCommitmentCommand> _validator;
        private readonly ICommitmentRepository _commitmentRepository;

        public CreateCommitmentCommandHandler(ICommitmentRepository commitmentRepository, AbstractValidator<CreateCommitmentCommand> validator)
        {
            _commitmentRepository = commitmentRepository;
            _validator = validator;
        }

        public async Task<long> Handle(CreateCommitmentCommand message)
        {
            Logger.Info(BuildInfoMessage(message));

            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var newCommitment = MapFrom(message.Commitment);

            return await _commitmentRepository.Create(newCommitment);
        }

        private Domain.Commitment MapFrom(Api.Types.Commitment commitment)
        {
            var domainCommitment = new Commitment
            {
                Id = commitment.Id,
                Name = commitment.Name,
                EmployerAccountId = commitment.EmployerAccountId,
                LegalEntityId = commitment.LegalEntityId,
                LegalEntityName = commitment.LegalEntityName,
                ProviderId = commitment.ProviderId,
                ProviderName = commitment.ProviderName,
                Status = CommitmentStatus.Draft,
                Apprenticeships = commitment.Apprenticeships.Select(x => new Apprenticeship
                {
                    Id = x.Id,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    ULN = x.ULN,
                    CommitmentId = commitment.Id,
                    Status = (Domain.ApprenticeshipStatus)x.Status,
                    AgreementStatus = (Domain.AgreementStatus)x.AgreementStatus,
                    TrainingId = x.TrainingId,
                    Cost = x.Cost,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate
                }).ToList()
            };

            return domainCommitment;
        }

        private string BuildInfoMessage(CreateCommitmentCommand cmd)
        {
            return $"Employer: {cmd.Commitment.EmployerAccountId} has called CreateCommitmentCommand";
        }
    }
}
