using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Domain.Data;

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
                Data = new List<OverlappingApprentice>()
            };

            var ulns = query.OverlappingApprenticeshipRequests.Select(x => x.Uln);

            var apprenticeships = await _apprenticeshipRepository.GetActiveApprenticeshipsByUlns(ulns);

            foreach (var request in query.OverlappingApprenticeshipRequests)
            {
                foreach (var apprenticeship in apprenticeships)
                {

                    if(_overlapRules.IsOverlap(request, apprenticeship))
                    {
                        result.Data.Add(new OverlappingApprentice
                        {
                            //todo: flesh out
                        });
                    }

                }
            }

            return result;
        }
    }
}
