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

        public async Task Resolve(long? apprenticeshipId, long? draftApprenticeshipId, OverlappingTrainingDateRequestResolutionType resolutionType)
        {
            OverlappingTrainingDateRequest result = null;
            var oltd = _dbContext.Value.OverlappingTrainingDateRequests
                .Include(r => r.DraftApprenticeship)
                .Include(r => r.PreviousApprenticeship);

            if (apprenticeshipId.HasValue && apprenticeshipId.Value > 0)
            {
                result = await oltd.SingleOrDefaultAsync(c => c.PreviousApprenticeshipId == apprenticeshipId.Value
                    && c.Status == OverlappingTrainingDateRequestStatus.Pending);
            }
            else if (draftApprenticeshipId.HasValue && draftApprenticeshipId.Value > 0)
            {
                result = await oltd.SingleOrDefaultAsync(c => c.DraftApprenticeshipId == draftApprenticeshipId.Value
                    && c.Status == OverlappingTrainingDateRequestStatus.Pending);
            }
            else
            {
                throw new InvalidOperationException("Draft apprenticeship and apprenticeship ids are null");
            }

            await CheckAndResolveOverlap(result, resolutionType);
        }

        public async Task DraftApprenticeshpDeleted(long draftApprenticeshipId, OverlappingTrainingDateRequestResolutionType resolutionType)
        {
            var result = await _dbContext.Value.OverlappingTrainingDateRequests
               .Include(r => r.PreviousApprenticeship)
               .SingleOrDefaultAsync(c => c.DraftApprenticeshipId == draftApprenticeshipId
               && c.Status == OverlappingTrainingDateRequestStatus.Pending);

            if (result != null)
            {
                Resolve(result, resolutionType);
            }
        }

        private async Task CheckAndResolveOverlap(OverlappingTrainingDateRequest overlappingTrainingDateRequest, OverlappingTrainingDateRequestResolutionType resolutionType)
        {
            if (overlappingTrainingDateRequest != null)
            {
                _logger.LogInformation($"OverlappingTrainingDateRequest found Apprenticeship-Id:{overlappingTrainingDateRequest.PreviousApprenticeshipId}, DraftApprenticeshipId : {overlappingTrainingDateRequest.DraftApprenticeshipId}");

                if (await CheckCanResolveOverlap(overlappingTrainingDateRequest, resolutionType))
                {
                    _logger.LogInformation($"OverlappingTrainingDateRequest Resolving Apprenticeship-Id:{overlappingTrainingDateRequest.PreviousApprenticeshipId}, DraftApprenticeshipId : {overlappingTrainingDateRequest.DraftApprenticeshipId}");
                    Resolve(overlappingTrainingDateRequest, resolutionType);
                }
            }
        }

        private void Resolve(OverlappingTrainingDateRequest overlappingTrainingDateRequest, OverlappingTrainingDateRequestResolutionType resolutionType)
        {
            var apprenticeship = overlappingTrainingDateRequest.PreviousApprenticeship;
            apprenticeship.ResolveTrainingDateRequest(overlappingTrainingDateRequest.DraftApprenticeshipId, resolutionType);
            _logger.LogInformation($"OverlappingTrainingDateRequest resolved Apprenticeship-Id:{apprenticeship.Id}, DraftApprenticeshipId : {overlappingTrainingDateRequest.DraftApprenticeshipId}");
        }

        private async Task<bool> CheckCanResolveOverlap(OverlappingTrainingDateRequest overlappingTrainingDateRequest, OverlappingTrainingDateRequestResolutionType resolutionType)
        {
            if (Mandatory_Fields_Missing(overlappingTrainingDateRequest))
            {
                _logger.LogInformation($"OverlappingTrainingDateRequest Mandatory field missing, Apprenticeship-Id:{overlappingTrainingDateRequest.PreviousApprenticeshipId}, DraftApprenticeshipId : {overlappingTrainingDateRequest.DraftApprenticeshipId}");
                // resolve overlap if any of the mandatory fields missing
                return true;
            }

            if (resolutionType == OverlappingTrainingDateRequestResolutionType.ApprentieshipIsStillActive)
                return true; // resolve if employer has confirmed that the apprenticeship Is Stil lActive

            if (ULN_Changed(overlappingTrainingDateRequest))
            {
                _logger.LogInformation($"OverlappingTrainingDateRequest Uln changed, Apprenticeship-Id:{overlappingTrainingDateRequest.PreviousApprenticeshipId}, DraftApprenticeshipId : {overlappingTrainingDateRequest.DraftApprenticeshipId}");
                // resolve overlap if the draft apprenticeship uln has changed.
                return true;
            }
            if (OverlapCheckRequired(resolutionType) &&
                await IsThereStillAOverlap(overlappingTrainingDateRequest))
            {
                _logger.LogInformation($"OverlappingTrainingDateRequest not resolving, Apprenticeship-Id:{overlappingTrainingDateRequest.PreviousApprenticeshipId}, DraftApprenticeshipId : {overlappingTrainingDateRequest.DraftApprenticeshipId}");
                // Don't resolve if there is still an overlap.
                return false;
            }

            return true;
        }

        private bool ULN_Changed(OverlappingTrainingDateRequest overlappingTrainingDateRequest)
        {
            return overlappingTrainingDateRequest.DraftApprenticeship.Uln != overlappingTrainingDateRequest.PreviousApprenticeship.Uln;
        }

        private bool Mandatory_Fields_Missing(OverlappingTrainingDateRequest overlappingTrainingDateRequest)
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

            var isThereStillAoverlap = result != null
                && result.HasOverlappingStartDate
                && result.ApprenticeshipId == overlappingTrainingDateRequestAggregate.PreviousApprenticeshipId;

            if (isThereStillAoverlap)
            {
                _logger.LogInformation($"OverlappingTrainingDateRequest - still overlap persent, Apprenticeship-Id {overlappingTrainingDateRequestAggregate.PreviousApprenticeshipId}, DraftApprenticeshipId - {overlappingTrainingDateRequestAggregate.DraftApprenticeshipId}");
            }
            else
            {
                _logger.LogInformation($"OverlappingTrainingDateRequest - no overlap found, Apprenticeship-Id {overlappingTrainingDateRequestAggregate.PreviousApprenticeshipId}, DraftApprenticeshipId - {overlappingTrainingDateRequestAggregate.DraftApprenticeshipId}");
            }

            return isThereStillAoverlap;
        }

        private bool OverlapCheckRequired(OverlappingTrainingDateRequestResolutionType resolutionType)
        {
            switch (resolutionType)
            {
                case OverlappingTrainingDateRequestResolutionType.ApprenticeshipStopped:
                case OverlappingTrainingDateRequestResolutionType.StopDateUpdate:
                    _logger.LogInformation($"OverlappingTrainingDateRequest overlapcheck is not required as the resolution type is {resolutionType}");
                    return false;
            }

            _logger.LogInformation($"OverlappingTrainingDateRequest overlapcheck is required as the resolution type is {resolutionType}");
            return true;
        }
    }
}