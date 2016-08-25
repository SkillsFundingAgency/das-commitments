using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain.Data;

namespace SFA.DAS.Commitments.Application.Queries.GetCommitment
{
    public sealed class GetCommitmentQueryHandler : IAsyncRequestHandler<GetCommitmentRequest, GetCommitmentResponse>
    {
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly AbstractValidator<GetCommitmentRequest> _validator;

        public GetCommitmentQueryHandler(ICommitmentRepository commitmentRepository, AbstractValidator<GetCommitmentRequest> validator)
        {
            _commitmentRepository = commitmentRepository;
            _validator = validator;
        }

        public async Task<GetCommitmentResponse> Handle(GetCommitmentRequest message)
        {
            if (!_validator.Validate(message).IsValid)
            {
                throw new InvalidRequestException();
            }

            var commitment = await _commitmentRepository.GetById(message.CommitmentId);

            if (commitment == null)
            {
                return new GetCommitmentResponse { Data = null };
            }

            CheckAuthorization(message, commitment);

            return MapResponseFrom(commitment);
        }

        private static void CheckAuthorization(GetCommitmentRequest message, Domain.Commitment commitment)
        {
            if (message.ProviderId.HasValue && commitment.ProviderId != message.ProviderId)
            {
                throw new UnauthorizedException($"Provider unauthorized to view commitment: {commitment.Id}");
            }

            if (message.AccountId.HasValue && commitment.EmployerAccountId != message.AccountId)
            {
                throw new UnauthorizedException($"Employer unauthorized to view commitment: {commitment.Id}");
            }
        }

        private static GetCommitmentResponse MapResponseFrom(Domain.Commitment commitment)
        {
            return new GetCommitmentResponse
            {
                Data = new Commitment
                {
                    Id = commitment.Id,
                    Name = commitment.Name,
                    ProviderId = commitment.ProviderId,
                    ProviderName = "",
                    EmployerAccountId = commitment.EmployerAccountId,
                    EmployerAccountName = "",
                    LegalEntityId = commitment.LegalEntityId,
                    LegalEntityName = "",
                    Apprenticeships = commitment?.Apprenticeships?.Select(x => new Apprenticeship
                    {
                        Id = x.Id,
                        ULN = x.ULN,
                        ApprenticeName = x.ApprenticeName,
                        CommitmentId = x.CommitmentId,
                        TrainingId = x.TrainingId,
                        Cost = x.Cost,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        AgreementStatus = (AgreementStatus)x.AgreementStatus,
                        Status = (ApprenticeshipStatus)x.Status
                    }).ToList()
                }
            };
        }
    }
}
