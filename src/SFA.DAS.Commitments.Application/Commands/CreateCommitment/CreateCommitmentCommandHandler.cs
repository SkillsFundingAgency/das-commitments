using System.Linq;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain.Data;

namespace SFA.DAS.Commitments.Application.Commands.CreateCommitment
{
    public sealed class CreateCommitmentCommandHandler : IAsyncRequestHandler<CreateCommitmentCommand, long>
    {
        private CreateCommitmentValidator _createCommitmentValidator;
        private ICommitmentRepository _commitmentRepository;

        public CreateCommitmentCommandHandler(ICommitmentRepository commitmentRepository, CreateCommitmentValidator createCommitmentValidator)
        {
            _commitmentRepository = commitmentRepository;
            _createCommitmentValidator = createCommitmentValidator;
        }

        public async Task<long> Handle(CreateCommitmentCommand message)
        {
            if (!_createCommitmentValidator.Validate(message).IsValid)
            {
                throw new InvalidRequestException();
            }

            return await _commitmentRepository.Create(MapFrom(message.Commitment));
        }

        private Domain.Commitment MapFrom(Commitment commitment)
        {
            var domainCommitment = new Domain.Commitment
            {
                Id = commitment.Id,
                Name = commitment.Name,
                EmployerAccountId = commitment.EmployerAccountId,
                LegalEntityId = commitment.LegalEntityId,
                ProviderId = commitment.ProviderId,
                Apprenticeships = commitment.Apprenticeships.Select(x => new Domain.Apprenticeship
                {
                    Id = x.Id,
                    ApprenticeName = x.ApprenticeName,
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
    }
}
