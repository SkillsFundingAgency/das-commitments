using System;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
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
                Data = result == null ? null : MapFrom(result)
            };
        }

        private Api.Types.Apprenticeship.ApprenticeshipUpdate MapFrom(ApprenticeshipUpdate source)
        {
            return new Api.Types.Apprenticeship.ApprenticeshipUpdate
            {
                Id = source.Id,
                ApprenticeshipId = source.ApprenticeshipId,
                Originator = (Api.Types.Apprenticeship.Types.Originator) source.Originator,
                FirstName = source.FirstName,
                LastName = source.LastName,
                DateOfBirth = source.DateOfBirth,
                ULN = source.ULN,
                TrainingCode = source.TrainingCode,
                TrainingType = source.TrainingType.HasValue ? (Api.Types.Apprenticeship.Types.TrainingType) source.TrainingType
                                                            : default(Api.Types.Apprenticeship.Types.TrainingType?),
                TrainingName = source.TrainingName,
                Cost = source.Cost,
                StartDate = source.StartDate,
                EndDate = source.EndDate
            };
        }

        private void CheckAuthorization(GetPendingApprenticeshipUpdateRequest request, Apprenticeship apprenticeship)
        {
            switch (request.Caller.CallerType)
            {
                case CallerType.Provider:
                    if (apprenticeship.ProviderId != request.Caller.Id)
                        throw new UnauthorizedException($"Provider {request.Caller.Id} unauthorized to view apprenticeship {request.ApprenticeshipId}");
                    break;
                case CallerType.Employer:
                default:
                    if (apprenticeship.EmployerAccountId != request.Caller.Id)
                        throw new UnauthorizedException($"Employer {request.Caller.Id} unauthorized to view apprenticeship {request.ApprenticeshipId}");
                    break;
            }
        }

    }
}
