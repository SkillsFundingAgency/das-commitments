using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;

using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.Validation;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.Queries.GetOverlappingApprenticeships
{
    public sealed class GetOverlappingApprenticeshipsQueryHandler : IAsyncRequestHandler<GetOverlappingApprenticeshipsRequest, GetOverlappingApprenticeshipsResponse>
    {
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly IApprenticeshipOverlapRules _overlapRules;
        private readonly ICommitmentsLogger _logger;
        private readonly AbstractValidator<GetOverlappingApprenticeshipsRequest> _validator;

        public GetOverlappingApprenticeshipsQueryHandler(IApprenticeshipRepository apprenticeshipRepository, AbstractValidator<GetOverlappingApprenticeshipsRequest> validator, IApprenticeshipOverlapRules overlapRules, ICommitmentsLogger logger)
        {
            _apprenticeshipRepository = apprenticeshipRepository;
            _validator = validator;
            _overlapRules = overlapRules;
            _logger = logger;
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
                Data = new List<ApprenticeshipResult>()
            };
            
            var ulns = query.OverlappingApprenticeshipRequests.Where(x => !string.IsNullOrWhiteSpace(x.Uln)).Select(x => x.Uln).ToList();

            if (!ulns.Any())
                return result;

            var apprenticeships = await _apprenticeshipRepository.GetActiveApprenticeshipsByUlns(ulns);
            foreach (var apprenticeship in apprenticeships)
            {
                foreach (var request in query.OverlappingApprenticeshipRequests.Where(x => x.Uln == apprenticeship.Uln))
                {
                    var validationFailReason = _overlapRules.DetermineOverlap(request, apprenticeship);

                    if (validationFailReason != ValidationFailReason.None)
                    {

                        _logger.Info($"ULN: {request.StartDate:MMM yyyy} - {request.EndDate:MMM yyyy} Reason: {validationFailReason} " +                        
                                     $"with Apprenticeship Id: {apprenticeship.Id} {apprenticeship.StartDate:MMM yyyy} - {apprenticeship.EndDate:MMM yyyy}");
                        apprenticeship.ValidationFailReason = validationFailReason;
                        apprenticeship.RequestApprenticeshipId = request.ApprenticeshipId;
                        result.Data.Add(apprenticeship);
                    }
                }
            }

            return result;
        }
    }
}
