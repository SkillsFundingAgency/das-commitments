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
            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var commitment = await _commitmentRepository.GetById(message.CommitmentId);

            if (commitment == null)
            {
                return new GetApprenticeshipResponse();
            }

            CheckAuthorization(message, commitment);

            var matchingApprenticeship = commitment.Apprenticeships.SingleOrDefault(x => x.Id == message.ApprenticeshipId);

            return MapResponseFrom(matchingApprenticeship);
        }

        private static void CheckAuthorization(GetApprenticeshipRequest message, Commitment commitment)
        {
            switch (message.Caller.CallerType)
            {
                case CallerType.Provider:
                    if (commitment.ProviderId != message.Caller.Id)
                        throw new UnauthorizedException($"Provider unauthorized to view apprenticeship: {message.ApprenticeshipId}");
                    break;
                case CallerType.Employer:
                default:
                    if (commitment.EmployerAccountId != message.Caller.Id)
                        throw new UnauthorizedException($"Employer unauthorized to view apprenticeship: {message.ApprenticeshipId}");
                    break;
            }
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
                CommitmentId = matchingApprenticeship.CommitmentId,
                FirstName = matchingApprenticeship.FirstName,
                LastName = matchingApprenticeship.LastName,
                ULN = matchingApprenticeship.ULN,
                TrainingType = (Api.Types.TrainingType)matchingApprenticeship.TrainingType,
                TrainingCode = matchingApprenticeship.TrainingCode,
                TrainingName = matchingApprenticeship.TrainingName,
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
