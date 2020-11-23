using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Queries.GetApprenticeship
{
    public sealed class GetApprenticeshipQueryHandler : IAsyncRequestHandler<GetApprenticeshipRequest, GetApprenticeshipResponse>
    {
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly AbstractValidator<GetApprenticeshipRequest> _validator;

        public GetApprenticeshipQueryHandler(IApprenticeshipRepository apprenticeshipRepository, AbstractValidator<GetApprenticeshipRequest> validator)
        {
            _apprenticeshipRepository = apprenticeshipRepository;
            _validator = validator;
        }

        public async Task<GetApprenticeshipResponse> Handle(GetApprenticeshipRequest message)
        {
            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var apprenticeship = await _apprenticeshipRepository.GetApprenticeship(message.ApprenticeshipId);

            if (apprenticeship == null)
            {
                return new GetApprenticeshipResponse();
            }

            CheckAuthorization(message, apprenticeship);

            apprenticeship.ChangeOfPartyRequests = await _apprenticeshipRepository.GetChangeOfPartyResponse(message.ApprenticeshipId);

            return new GetApprenticeshipResponse
            {
                Data = apprenticeship
            };
        }

        private static void CheckAuthorization(GetApprenticeshipRequest message, Apprenticeship apprenticeship)
        {
            switch (message.Caller.CallerType)
            {
                case CallerType.Provider:
                    if (apprenticeship.ProviderId != message.Caller.Id)
                        throw new UnauthorizedException($"Provider {message.Caller.Id} not authorised to access apprenticeship {message.ApprenticeshipId}, expected provider {apprenticeship.ProviderId}");
                    break;
                case CallerType.Employer:
                default:
                    if (apprenticeship.EmployerAccountId != message.Caller.Id)
                        throw new UnauthorizedException($"Employer {message.Caller.Id} not authorised to access apprenticeship {message.ApprenticeshipId}, expected employer {apprenticeship.EmployerAccountId}");
                    break;
            }
        }
    }
}
