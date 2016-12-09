using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Application.Rules;
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

        private readonly ICommitmentRules _commitmentRules;

        public GetCommitmentQueryHandler(
            ICommitmentRepository commitmentRepository, 
            AbstractValidator<GetCommitmentRequest> validator,
            ICommitmentRules commitmentRules)
        {
            _commitmentRepository = commitmentRepository;
            _validator = validator;
            _commitmentRules = commitmentRules;
        }

        public async Task<GetCommitmentResponse> Handle(GetCommitmentRequest message)
        {
            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var commitment = await _commitmentRepository.GetCommitmentById(message.CommitmentId);

            if (commitment == null)
            {
                return new GetCommitmentResponse { Data = null };
            }

            CheckAuthorization(message, commitment);


            return MapResponseFrom(commitment, message.Caller.CallerType);
        }

        private GetCommitmentResponse MapResponseFrom(Domain.Entities.Commitment commitment, CallerType callerType)
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
                    LegalEntityId = commitment.LegalEntityId,
                    LegalEntityName = commitment.LegalEntityName,
                    CommitmentStatus = (CommitmentStatus)commitment.CommitmentStatus,
                    EditStatus = (EditStatus)commitment.EditStatus,
                    AgreementStatus = _commitmentRules.DetermineAgreementStatus(commitment.Apprenticeships),
                    LastAction = (LastAction)commitment.LastAction,
                    CanBeApproved = callerType == CallerType.Employer ? commitment.EmployerCanApproveCommitment : commitment.ProviderCanApproveCommitment,
                    Apprenticeships = commitment.Apprenticeships?.Select(x => new Apprenticeship
                    {
                        Id = x.Id,
                        ULN = x.ULN,
                        CommitmentId = x.CommitmentId,
                        EmployerAccountId = x.EmployerAccountId,
                        ProviderId = x.ProviderId,
                        Reference = x.Reference,
                        FirstName = x.FirstName,
                        LastName = x.LastName,
                        TrainingType = (TrainingType)x.TrainingType,
                        TrainingCode = x.TrainingCode,
                        TrainingName = x.TrainingName,
                        Cost = x.Cost,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        AgreementStatus = (AgreementStatus)x.AgreementStatus,
                        PaymentStatus = (PaymentStatus)x.PaymentStatus,
                        DateOfBirth = x.DateOfBirth,
                        NINumber = x.NINumber,
                        EmployerRef = x.EmployerRef,
                        ProviderRef = x.ProviderRef,
                        CanBeApproved = callerType == CallerType.Employer ? x.EmployerCanApproveApprenticeship : x.ProviderCanApproveApprenticeship
                    }).ToList()
                }
            };
        }

        private static void CheckAuthorization(GetCommitmentRequest message, Domain.Entities.Commitment commitment)
        {
            switch (message.Caller.CallerType)
            {
                case CallerType.Provider:
                    if (commitment.ProviderId != message.Caller.Id)
                        throw new UnauthorizedException($"Provider {message.Caller.Id} unauthorized to view commitment {message.CommitmentId}");
                    break;
                case CallerType.Employer:
                default:
                    if (commitment.EmployerAccountId != message.Caller.Id)
                        throw new UnauthorizedException($"Employer {message.Caller.Id} unauthorized to view commitment {message.CommitmentId}");
                    break;
            }
        }
    }
}
