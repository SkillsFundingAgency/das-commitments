using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Api.Types.Validation;
using SFA.DAS.Commitments.Api.Types.Validation.Types;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Queries.GetOverlappingApprenticeships
{
    public sealed class GetOverlappingApprenticeshipsQueryHandler : IAsyncRequestHandler<GetOverlappingApprenticeshipsRequest, GetOverlappingApprenticeshipsResponse>
    {
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly IApprenticeshipOverlapRules _overlapRules;
        private readonly AbstractValidator<GetOverlappingApprenticeshipsRequest> _validator;

        public GetOverlappingApprenticeshipsQueryHandler(IApprenticeshipRepository apprenticeshipRepository, AbstractValidator<GetOverlappingApprenticeshipsRequest> validator, IApprenticeshipOverlapRules overlapRules)
        {
            _apprenticeshipRepository = apprenticeshipRepository;
            _validator = validator;
            _overlapRules = overlapRules;
        }

        public async Task<GetOverlappingApprenticeshipsResponse> Handle(GetOverlappingApprenticeshipsRequest query)
        {
            var validationResult = _validator.Validate(query);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var result = new GetOverlappingApprenticeshipsResponse
            {
                Data = new List<OverlappingApprenticeship>()
            };

            var ulns = query.OverlappingApprenticeshipRequests.Select(x => x.Uln);

            var apprenticeships = await _apprenticeshipRepository.GetActiveApprenticeshipsByUlns(ulns);

            foreach (var request in query.OverlappingApprenticeshipRequests)
            {
                foreach (var apprenticeship in apprenticeships)
                {
                    var validationFailReason = _overlapRules.DetermineOverlap(request, apprenticeship);

                    if (validationFailReason != ValidationFailReason.None)
                    {
                        result.Data.Add(MapFrom(apprenticeship, validationFailReason));
                    }
                }
            }

            return result;
        }

        private OverlappingApprenticeship MapFrom(ApprenticeshipResult source, ValidationFailReason validationFailReason)
        {
            var result = new OverlappingApprenticeship
            {
                Apprenticeship = new Api.Types.Apprenticeship.Apprenticeship
                {
                    Id = source.Id,
                    StartDate = source.StartDate,
                    EndDate = source.EndDate,
                    ULN = source.Uln
                    //todo: complete           
                },
                EmployerAccountId = source.EmployerAccountId,
                LegalEntityName = source.LegalEntityName,
                ProviderId = source.ProviderId,
                ProviderName = source.ProviderName,
                ValidationFailReason = validationFailReason
                
            };

            return result;
        }

    }
}
