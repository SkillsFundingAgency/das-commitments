using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Services;

public class ResolveOverlappingTrainingDateRequestService(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    IOverlapCheckService overlapCheckService,
    ILogger<ResolveOverlappingTrainingDateRequestService> logger)
    : IResolveOverlappingTrainingDateRequestService
{
    public async Task Resolve(long? apprenticeshipId, long? draftApprenticeshipId, OverlappingTrainingDateRequestResolutionType resolutionType)
    {
        var oltd = dbContext.Value.OverlappingTrainingDateRequests
            .Include(r => r.DraftApprenticeship)
            .Include(r => r.PreviousApprenticeship);

        if (apprenticeshipId.HasValue && apprenticeshipId.Value > 0)
        {
            var results = await oltd.Where(c => c.PreviousApprenticeshipId == apprenticeshipId.Value
                                                && c.Status == OverlappingTrainingDateRequestStatus.Pending).ToListAsync();

            foreach (var result in results)
            {
                await CheckAndResolveOverlap(result, resolutionType);
            }
        }
        else if (draftApprenticeshipId.HasValue && draftApprenticeshipId.Value > 0)
        {
            var result = await oltd.SingleOrDefaultAsync(c => c.DraftApprenticeshipId == draftApprenticeshipId.Value
                                                              && c.Status == OverlappingTrainingDateRequestStatus.Pending);
            await CheckAndResolveOverlap(result, resolutionType);
        }
        else
        {
            throw new InvalidOperationException("Draft apprenticeship and apprenticeship ids are null");
        }
    }

    public async Task DraftApprenticeshpDeleted(long draftApprenticeshipId, OverlappingTrainingDateRequestResolutionType resolutionType)
    {
        var result = await dbContext.Value.OverlappingTrainingDateRequests
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
            logger.LogInformation("OverlappingTrainingDateRequest found Apprenticeship-Id:{PreviousApprenticeshipId}, DraftApprenticeshipId : {DraftApprenticeshipId}", overlappingTrainingDateRequest.PreviousApprenticeshipId, overlappingTrainingDateRequest.DraftApprenticeshipId);

            if (await CheckCanResolveOverlap(overlappingTrainingDateRequest, resolutionType))
            {
                logger.LogInformation("OverlappingTrainingDateRequest Resolving Apprenticeship-Id:{PreviousApprenticeshipId}, DraftApprenticeshipId : {DraftApprenticeshipId}", overlappingTrainingDateRequest.PreviousApprenticeshipId, overlappingTrainingDateRequest.DraftApprenticeshipId);
                Resolve(overlappingTrainingDateRequest, resolutionType);
            }
        }
    }

    private void Resolve(OverlappingTrainingDateRequest overlappingTrainingDateRequest, OverlappingTrainingDateRequestResolutionType resolutionType)
    {
        var apprenticeship = overlappingTrainingDateRequest.PreviousApprenticeship;
        apprenticeship.ResolveTrainingDateRequest(overlappingTrainingDateRequest.DraftApprenticeshipId, resolutionType);
        logger.LogInformation("OverlappingTrainingDateRequest resolved Apprenticeship-Id:{Id}, DraftApprenticeshipId : {DraftApprenticeshipId}", apprenticeship.Id, overlappingTrainingDateRequest.DraftApprenticeshipId);
    }

    private async Task<bool> CheckCanResolveOverlap(OverlappingTrainingDateRequest overlappingTrainingDateRequest, OverlappingTrainingDateRequestResolutionType resolutionType)
    {
        if (Mandatory_Fields_Missing(overlappingTrainingDateRequest))
        {
            logger.LogInformation("OverlappingTrainingDateRequest Mandatory field missing, Apprenticeship-Id:{PreviousApprenticeshipId}, DraftApprenticeshipId : {DraftApprenticeshipId}", overlappingTrainingDateRequest.PreviousApprenticeshipId, overlappingTrainingDateRequest.DraftApprenticeshipId);
            // resolve overlap if any of the mandatory fields missing
            return true;
        }

        if (resolutionType == OverlappingTrainingDateRequestResolutionType.ApprenticeshipIsStillActive)
        {
            logger.LogInformation("OverlappingTrainingDateRequest  employer confirm that Apprenticeship is still active, Apprenticeship-Id:{PreviousApprenticeshipId}, DraftApprenticeshipId : {DraftApprenticeshipId}", overlappingTrainingDateRequest.PreviousApprenticeshipId, overlappingTrainingDateRequest.DraftApprenticeshipId);
            return true; // resolve if employer has confirmed that the apprenticeship Is Stil lActive
        }

        if (resolutionType == OverlappingTrainingDateRequestResolutionType.ApprenticeshipStopDateIsCorrect)
        {
            logger.LogInformation("OverlappingTrainingDateRequest  employer confirm that Apprenticeship Stop Date Is Correct, Apprenticeship-Id:{PreviousApprenticeshipId}, DraftApprenticeshipId : {DraftApprenticeshipId}", overlappingTrainingDateRequest.PreviousApprenticeshipId, overlappingTrainingDateRequest.DraftApprenticeshipId);
            return true; // resolve if employer has confirmed that the apprenticeship Is Stil lActive
        }

        if (resolutionType == OverlappingTrainingDateRequestResolutionType.ApprenticeshipEndDateIsCorrect)
        {
            logger.LogInformation("OverlappingTrainingDateRequest  employer confirm that Apprenticeship End Date Is Correct, Apprenticeship-Id:{PreviousApprenticeshipId}, DraftApprenticeshipId : {DraftApprenticeshipId}", overlappingTrainingDateRequest.PreviousApprenticeshipId, overlappingTrainingDateRequest.DraftApprenticeshipId);
            return true; // resolve if employer has confirmed that the apprenticeship Is Stil lActive
        }

        if (ULN_Changed(overlappingTrainingDateRequest))
        {
            logger.LogInformation("OverlappingTrainingDateRequest Uln changed, Apprenticeship-Id:{PreviousApprenticeshipId}, DraftApprenticeshipId : {DraftApprenticeshipId}", overlappingTrainingDateRequest.PreviousApprenticeshipId, overlappingTrainingDateRequest.DraftApprenticeshipId);
            // resolve overlap if the draft apprenticeship uln has changed.
            return true;
        }

        if (OverlapCheckRequired(resolutionType) && await IsThereStillAOverlap(overlappingTrainingDateRequest))
        {
            logger.LogInformation("OverlappingTrainingDateRequest not resolving, Apprenticeship-Id:{PreviousApprenticeshipId}, DraftApprenticeshipId : {DraftApprenticeshipId}", overlappingTrainingDateRequest.PreviousApprenticeshipId, overlappingTrainingDateRequest.DraftApprenticeshipId);
            // Don't resolve if there is still an overlap.
            return false;
        }

        return true;
    }

    private static bool ULN_Changed(OverlappingTrainingDateRequest overlappingTrainingDateRequest)
    {
        return overlappingTrainingDateRequest.DraftApprenticeship.Uln != overlappingTrainingDateRequest.PreviousApprenticeship.Uln;
    }

    private static bool Mandatory_Fields_Missing(OverlappingTrainingDateRequest overlappingTrainingDateRequest)
    {
        var draftApprenticeship = overlappingTrainingDateRequest.DraftApprenticeship;
        return !draftApprenticeship.StartDate.HasValue || !draftApprenticeship.EndDate.HasValue || string.IsNullOrWhiteSpace(draftApprenticeship.Uln);
    }

    private async Task<bool> IsThereStillAOverlap(OverlappingTrainingDateRequest overlappingTrainingDateRequestAggregate)
    {
        var apprenticeship = overlappingTrainingDateRequestAggregate.PreviousApprenticeship;
        var result = await overlapCheckService.CheckForOverlapsOnStartDate(apprenticeship.Uln,
            new Domain.Entities.DateRange(overlappingTrainingDateRequestAggregate.DraftApprenticeship.StartDate.Value, overlappingTrainingDateRequestAggregate.DraftApprenticeship.EndDate.Value),
            null,
            CancellationToken.None);

        var isThereStillAoverlap = result != null
                                   && result.HasOverlappingStartDate
                                   && result.ApprenticeshipId == overlappingTrainingDateRequestAggregate.PreviousApprenticeshipId;

        if (isThereStillAoverlap)
        {
            logger.LogInformation("OverlappingTrainingDateRequest - still overlap present, Apprenticeship-Id {PreviousApprenticeshipId}, DraftApprenticeshipId - {DraftApprenticeshipId}",
                overlappingTrainingDateRequestAggregate.PreviousApprenticeshipId, overlappingTrainingDateRequestAggregate.DraftApprenticeshipId);
        }
        else
        {
            logger.LogInformation("OverlappingTrainingDateRequest - no overlap found, Apprenticeship-Id {PreviousApprenticeshipId}, DraftApprenticeshipId - {DraftApprenticeshipId}",
                overlappingTrainingDateRequestAggregate.PreviousApprenticeshipId, overlappingTrainingDateRequestAggregate.DraftApprenticeshipId);
        }

        return isThereStillAoverlap;
    }

    private bool OverlapCheckRequired(OverlappingTrainingDateRequestResolutionType resolutionType)
    {
        switch (resolutionType)
        {
            case OverlappingTrainingDateRequestResolutionType.ApprenticeshipStopped:
            case OverlappingTrainingDateRequestResolutionType.StopDateUpdate:
                logger.LogInformation("OverlappingTrainingDateRequest overlapcheck is not required as the resolution type is {ResolutionType}", resolutionType);
                return false;

            case OverlappingTrainingDateRequestResolutionType.ApprenticeshipEndDateUpdate:
                logger.LogInformation("OverlappingTrainingDateRequest overlapcheck is not required as the resolution type is {ResolutionType}", resolutionType);
                return false;
        }

        logger.LogInformation("OverlappingTrainingDateRequest overlapcheck is required as the resolution type is {ResolutionType}", resolutionType);

        return true;
    }
}