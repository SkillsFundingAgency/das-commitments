using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class OverlapCheckService : IOverlapCheckService
    {
        private readonly IUlnUtilisationService _ulnUtilisationService;

        public OverlapCheckService(IUlnUtilisationService ulnUtilisationService)
        {
            _ulnUtilisationService = ulnUtilisationService;
        }

        public async Task<OverlapCheckResult> CheckForOverlaps(string uln, DateTime startDate, DateTime endDate, long? existingApprenticeshipId, CancellationToken cancellationToken)
        {
            var ulnUtilisations = await _ulnUtilisationService.GetUlnUtilisations(uln, cancellationToken);

            var overlapStartDate = false;
            var overlapEndDate = false;


            foreach (var utilisation in ulnUtilisations.Where(x =>!existingApprenticeshipId.HasValue || x.ApprenticeshipId != existingApprenticeshipId.Value))
            {
                var overlapStatus = utilisation.DetermineOverlap(startDate, endDate);

                switch (overlapStatus)
                {
                    case OverlapStatus.OverlappingStartDate: overlapStartDate = true;
                        break;
                    case OverlapStatus.OverlappingEndDate: overlapEndDate = true;
                        break;
                    case OverlapStatus.DateWithin:
                    case OverlapStatus.DateEmbrace:
                        overlapStartDate = true;
                        overlapEndDate = true;
                        break;
                    default:
                        break;
                }

                if (overlapStartDate && overlapEndDate)
                {
                    break;
                }
            }

            return new OverlapCheckResult(overlapStartDate, overlapEndDate);
        }
    }
}
