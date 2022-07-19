using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class ResolveOverlappingTrainingDateRequestService : IResolveOverlappingTrainingDateRequestService
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly IOverlapCheckService _overlapCheckService;
        private readonly ILogger<ResolveOverlappingTrainingDateRequestService> _logger;

        public ResolveOverlappingTrainingDateRequestService(Lazy<ProviderCommitmentsDbContext> dbContext, 
            IOverlapCheckService overlapCheckService,
            ILogger<ResolveOverlappingTrainingDateRequestService> logger)
        {
            _dbContext = dbContext;
            _overlapCheckService = overlapCheckService;
            _logger = logger;
        }

        public async Task Resolve(long apprenticeshipId, OverlappingTrainingDateRequestResolutionType resolutionType)
        {
            var overlappingTrainingDateRequestAggregate = await _dbContext.Value.GetOverlappingTrainingDateRequestAggregate(apprenticeshipId, CancellationToken.None);
            if (overlappingTrainingDateRequestAggregate != null)
            {
                _logger.LogInformation($"OverlappingTrainingDateRequest found Apprenticeship-Id:{apprenticeshipId}, DraftApprenticeshipId : {overlappingTrainingDateRequestAggregate.DraftApprenticeshipId}");
                var apprenticeship = overlappingTrainingDateRequestAggregate.PreviousApprenticeship;

                if (await IsThereStillAOverlap(overlappingTrainingDateRequestAggregate, apprenticeship))
                {
                    // Don't resolve if there is still an overlap.
                    return;
                }

                apprenticeship.ResolveTrainingDateRequest(overlappingTrainingDateRequestAggregate.DraftApprenticeship, resolutionType, CancellationToken.None);
                _logger.LogInformation($"OverlappingTrainingDateRequest resolved Apprenticeship-Id:{apprenticeshipId}, DraftApprenticeshipId : {overlappingTrainingDateRequestAggregate.DraftApprenticeshipId}");
            }
        }

        private async Task<bool> IsThereStillAOverlap(OverlappingTrainingDateRequest overlappingTrainingDateRequestAggregate, Apprenticeship apprenticeship)
        {
            var result = await _overlapCheckService.CheckForOverlapsOnStartDate(apprenticeship.Uln,
                new Domain.Entities.DateRange(overlappingTrainingDateRequestAggregate.DraftApprenticeship.StartDate.Value, overlappingTrainingDateRequestAggregate.DraftApprenticeship.EndDate.Value),
                null,
                CancellationToken.None);

            return result != null && result.HasOverlappingStartDate;
        }
    }
}
