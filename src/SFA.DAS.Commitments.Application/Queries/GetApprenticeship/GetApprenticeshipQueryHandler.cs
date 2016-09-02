using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;

namespace SFA.DAS.Commitments.Application.Queries.GetApprenticeship
{
    public sealed class GetApprenticeshipQueryHandler : IAsyncRequestHandler<GetApprenticeshipRequest, GetApprenticeshipResponse>
    {
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly AbstractValidator<GetApprenticeshipRequest> _validator;

        public GetApprenticeshipQueryHandler(ICommitmentRepository commitmentRepository, AbstractValidator<GetApprenticeshipRequest> validator)
        {
            _commitmentRepository = commitmentRepository;
            _validator = validator;
        }

        public async Task<GetApprenticeshipResponse> Handle(GetApprenticeshipRequest message)
        {
            if (!_validator.Validate(message).IsValid)
            {
                throw new InvalidRequestException();
            }

            var commitment = await _commitmentRepository.GetById(message.CommitmentId);

            if (commitment == null)
            {
                return new GetApprenticeshipResponse();
            }

            if (commitment.EmployerAccountId != message.AccountId)
            {
                throw new UnauthorizedException($"Employer unauthorized to view apprenticeship: {message.ApprenticeshipId}");
            }

            var matchingApprenticeship = commitment.Apprenticeships.SingleOrDefault(x => x.Id == message.ApprenticeshipId);

            return MapResponseFrom(matchingApprenticeship);
        }

        private GetApprenticeshipResponse MapResponseFrom(Apprenticeship matchingApprenticeship)
        {
            var response = new GetApprenticeshipResponse();

            if (matchingApprenticeship == null)
            {
                return response;
            }

            response.Data = new Api.Types.Apprenticeship
            {
                Id = matchingApprenticeship.Id,
                ApprenticeName = matchingApprenticeship.ApprenticeName,
                CommitmentId = matchingApprenticeship.CommitmentId,
                ULN = matchingApprenticeship.ULN,
                TrainingId = matchingApprenticeship.TrainingId,
                Cost = matchingApprenticeship.Cost,
                StartDate = matchingApprenticeship.StartDate,
                EndDate = matchingApprenticeship.EndDate,
                Status = (Api.Types.ApprenticeshipStatus)matchingApprenticeship.Status,
                AgreementStatus = (Api.Types.AgreementStatus)matchingApprenticeship.AgreementStatus
            };

            return response;
        }
    }
}
