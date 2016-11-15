using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using AgreementStatus = SFA.DAS.Commitments.Api.Types.AgreementStatus;
using Apprenticeship = SFA.DAS.Commitments.Api.Types.Apprenticeship;
using Commitment = SFA.DAS.Commitments.Api.Types.Commitment;
using CommitmentStatus = SFA.DAS.Commitments.Api.Types.CommitmentStatus;
using TrainingType = SFA.DAS.Commitments.Api.Types.TrainingType;

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
            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var commitment = await _commitmentRepository.GetById(message.CommitmentId);

            if (commitment == null)
            {
                return new GetCommitmentResponse { Data = null };
            }

            CheckAuthorization(message, commitment);

            return MapResponseFrom(commitment);
        }

        private static void CheckAuthorization(GetCommitmentRequest message, Domain.Entities.Commitment commitment)
        {
            switch (message.Caller.CallerType)
            {
                case CallerType.Provider:
                    if (commitment.ProviderId != message.Caller.Id)
                        throw new UnauthorizedException($"Provider unauthorized to view commitment: {message.CommitmentId}");
                    break;
                case CallerType.Employer:
                default:
                    if (commitment.EmployerAccountId != message.Caller.Id)
                        throw new UnauthorizedException($"Employer unauthorized to view commitment: {message.CommitmentId}");
                    break;
            }
        }

        private static GetCommitmentResponse MapResponseFrom(Domain.Entities.Commitment commitment)
        {
            return new GetCommitmentResponse
            {
                Data = new Commitment
                {
                    Id = commitment.Id,
                    Reference = commitment.Reference,
                    ProviderId = commitment.ProviderId,
                    ProviderName = commitment.ProviderName,
                    EmployerAccountId = commitment.EmployerAccountId,
                    EmployerAccountName = "",
                    LegalEntityId = commitment.LegalEntityId,
                    LegalEntityName = commitment.LegalEntityName,
                    CommitmentStatus = (CommitmentStatus)commitment.CommitmentStatus,
                    Apprenticeships = commitment?.Apprenticeships?.Select(x => new Apprenticeship
                    {
                        Id = x.Id,
                        ULN = x.ULN,
                        FirstName = x.FirstName,
                        LastName = x.LastName,
                        TrainingType = (TrainingType) x.TrainingType,
                        TrainingCode = x.TrainingCode,
                        TrainingName = x.TrainingName,
                        Cost = x.Cost,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        AgreementStatus = (AgreementStatus)x.AgreementStatus,
                        PaymentStatus = (PaymentStatus)x.PaymentStatus
                    }).ToList()
                }
            };
        }
    }
}
