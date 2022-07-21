using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
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

        public async Task ResolveByApprenticeship(long apprenticeshipId, OverlappingTrainingDateRequestResolutionType resolutionType)
        {
            var result = await _dbContext.Value.OverlappingTrainingDateRequests
                .Include(r => r.DraftApprenticeship)
                .Include(r => r.PreviousApprenticeship)
                .SingleOrDefaultAsync(c => c.PreviousApprenticeshipId == apprenticeshipId
                && c.Status == OverlappingTrainingDateRequestStatus.Pending);

            await ResolveOverlap(result, resolutionType);
        }

        public async Task ResolveByDraftApprenticeshp(long draftAppretniceshipId, OverlappingTrainingDateRequestResolutionType resolutionType)
        {
            var result = await _dbContext.Value.OverlappingTrainingDateRequests
               .Include(r => r.DraftApprenticeship)
               .Include(r => r.PreviousApprenticeship)
               .SingleOrDefaultAsync(c => c.DraftApprenticeshipId == draftAppretniceshipId
               && c.Status == OverlappingTrainingDateRequestStatus.Pending);

            await ResolveOverlap(result, resolutionType);
        }

        public async Task DraftApprenticeshpDeleted(long draftAppretniceshipId, OverlappingTrainingDateRequestResolutionType resolutionType)
        {
            var result = await _dbContext.Value.OverlappingTrainingDateRequests
               .Include(r => r.PreviousApprenticeship)
               .SingleOrDefaultAsync(c => c.DraftApprenticeshipId == draftAppretniceshipId
               && c.Status == OverlappingTrainingDateRequestStatus.Pending);

            if (result != null)
            {
                var apprenticeship = result.PreviousApprenticeship;
                apprenticeship.ResolveTrainingDateRequest(draftAppretniceshipId, resolutionType, CancellationToken.None);
                _logger.LogInformation($"OverlappingTrainingDateRequest resolved Apprenticeship-Id:{apprenticeship.Id}, DraftApprenticeshipId : {draftAppretniceshipId}");
            }
        }

        private async Task ResolveOverlap(OverlappingTrainingDateRequest overlappingTrainingDateRequest, OverlappingTrainingDateRequestResolutionType resolutionType)
        {
            if (overlappingTrainingDateRequest != null)
            {
                _logger.LogInformation($"OverlappingTrainingDateRequest found Apprenticeship-Id:{overlappingTrainingDateRequest.PreviousApprenticeshipId}, DraftApprenticeshipId : {overlappingTrainingDateRequest.DraftApprenticeshipId}");

                if (await CheckCanResolveOverlap(overlappingTrainingDateRequest))
                {
                    var apprenticeship = overlappingTrainingDateRequest.PreviousApprenticeship;
                    apprenticeship.ResolveTrainingDateRequest(overlappingTrainingDateRequest.DraftApprenticeship.Id, resolutionType, CancellationToken.None);
                    _logger.LogInformation($"OverlappingTrainingDateRequest resolved Apprenticeship-Id:{apprenticeship.Id}, DraftApprenticeshipId : {overlappingTrainingDateRequest.DraftApprenticeshipId}");
                }
            }
        }

        private async Task<bool> CheckCanResolveOverlap(OverlappingTrainingDateRequest overlappingTrainingDateRequest)
        {
            if (Has_StartDate_EndDate_UlN_Removed_On_DraftApprenticeship(overlappingTrainingDateRequest))
            {
                return true;
            }
            if (await IsThereStillAOverlap(overlappingTrainingDateRequest))
            {
                // Don't resolve if there is still an overlap.
                return false;
            }

            return true;
        }

        private bool Has_StartDate_EndDate_UlN_Removed_On_DraftApprenticeship(OverlappingTrainingDateRequest overlappingTrainingDateRequest)
        {
            var draftApprenticeship = overlappingTrainingDateRequest.DraftApprenticeship;
            if (!draftApprenticeship.StartDate.HasValue || !draftApprenticeship.EndDate.HasValue || string.IsNullOrWhiteSpace(draftApprenticeship.Uln))
            {
                return true;
            }

            return false;
        }

        private async Task<bool> IsThereStillAOverlap(OverlappingTrainingDateRequest overlappingTrainingDateRequestAggregate)
        {
            var apprenticeship = overlappingTrainingDateRequestAggregate.PreviousApprenticeship;
            var result = await _overlapCheckService.CheckForOverlapsOnStartDate(apprenticeship.Uln,
                new Domain.Entities.DateRange(overlappingTrainingDateRequestAggregate.DraftApprenticeship.StartDate.Value, overlappingTrainingDateRequestAggregate.DraftApprenticeship.EndDate.Value),
                null,
                CancellationToken.None);

            return result != null 
                && result.HasOverlappingStartDate 
                && result.ApprenticeshipId == overlappingTrainingDateRequestAggregate.PreviousApprenticeshipId;
        }
    }
}
