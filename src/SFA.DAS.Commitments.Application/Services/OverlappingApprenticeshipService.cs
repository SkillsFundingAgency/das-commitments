using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.Validation;

namespace SFA.DAS.Commitments.Application.Services
{
    internal class OverlappingApprenticeshipService
    {
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly IApprenticeshipOverlapRules _overlapRules;

        internal OverlappingApprenticeshipService(IApprenticeshipRepository apprenticeshipRepository, IApprenticeshipOverlapRules overlapRules)
        {
            _apprenticeshipRepository = apprenticeshipRepository;
            _overlapRules = overlapRules;
        }

        internal async Task<bool> CommitmentHasOverlappingApprenticeships(Commitment commitment)
        {
            var potentiallyOverlappingApprenticeships = await GetPotentiallyOverlappingApprenticeships(commitment);

            foreach (var potentiallyOverlappingApprenticeship in potentiallyOverlappingApprenticeships)
            {
                foreach (var commitmentApprenticeship in commitment.Apprenticeships.Where(x => x.ULN == potentiallyOverlappingApprenticeship.Uln))
                {
                    var overlapRequest = new ApprenticeshipOverlapValidationRequest { ApprenticeshipId = commitmentApprenticeship.Id, Uln = commitmentApprenticeship.ULN, StartDate = commitmentApprenticeship.StartDate.Value, EndDate = commitmentApprenticeship.EndDate.Value };
                    var validationFailReason = _overlapRules.DetermineOverlap(overlapRequest, potentiallyOverlappingApprenticeship);

                    if (validationFailReason != ValidationFailReason.None)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private async Task<IEnumerable<ApprenticeshipResult>> GetPotentiallyOverlappingApprenticeships(Commitment commitment)
        {
            var ulns = commitment.Apprenticeships.Select(x => x.ULN);
            var potentialOverlappingApprenticeships = await _apprenticeshipRepository.GetActiveApprenticeshipsByUlns(ulns);
            return potentialOverlappingApprenticeships;
        }
    }
}
