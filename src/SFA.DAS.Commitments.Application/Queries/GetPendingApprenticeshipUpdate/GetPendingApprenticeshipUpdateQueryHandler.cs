using System;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Queries.GetPendingApprenticeshipUpdate
{
    public class GetPendingApprenticeshipUpdateQueryHandler: IAsyncRequestHandler<GetPendingApprenticeshipUpdateRequest,GetPendingApprenticeshipUpdateResponse>
    {
        private readonly AbstractValidator<GetPendingApprenticeshipUpdateRequest> _validator;
        private readonly IApprenticeshipUpdateRepository _apprenticeshipUpdateRepository;
        private readonly IApprenticeshipRepository _apprenticeshipRepository;

        public GetPendingApprenticeshipUpdateQueryHandler(AbstractValidator<GetPendingApprenticeshipUpdateRequest> validator, IApprenticeshipUpdateRepository apprenticeshipUpdateRepository, IApprenticeshipRepository apprenticeshipRepository)
        {
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));
            if(apprenticeshipUpdateRepository==null)
                throw new ArgumentNullException(nameof(apprenticeshipUpdateRepository));
            if(apprenticeshipRepository==null)
                throw new ArgumentException(nameof(apprenticeshipRepository));

            _validator = validator;
            _apprenticeshipUpdateRepository = apprenticeshipUpdateRepository;
            _apprenticeshipRepository = apprenticeshipRepository;
        }

        public async Task<GetPendingApprenticeshipUpdateResponse> Handle(GetPendingApprenticeshipUpdateRequest message)
        {
            var validationResult = _validator.Validate(message);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var apprenticeship = await _apprenticeshipRepository.GetApprenticeship(message.ApprenticeshipId);

            CheckAuthorization(message, apprenticeship);

            var result = await _apprenticeshipUpdateRepository.GetPendingApprenticeshipUpdate(message.ApprenticeshipId);

            return new GetPendingApprenticeshipUpdateResponse
            {
                Data = result
            };
        }

        private void CheckAuthorization(GetPendingApprenticeshipUpdateRequest request, Apprenticeship apprenticeship)
        {
            switch (request.Caller.CallerType)
            {
                case CallerType.Provider:
                    if (apprenticeship.ProviderId != request.Caller.Id)
                        throw new UnauthorizedException($"Provider {request.Caller.Id} not authorised to access apprenticeship {request.ApprenticeshipId}, expected provider {apprenticeship.ProviderId}");
                    break;
                case CallerType.Employer:
                default:
                    if (apprenticeship.EmployerAccountId != request.Caller.Id)
                        throw new UnauthorizedException($"Employer {request.Caller.Id} not authorised to access apprenticeship {request.ApprenticeshipId}, expected employer {apprenticeship.EmployerAccountId}");
                    break;
            }
        }

    }
}
