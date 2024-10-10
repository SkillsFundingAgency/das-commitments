using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers.OverlappingTrainingDateRequest;

public class CohortDeletedWithPendingOverlappingTrainingDateEventHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    IResolveOverlappingTrainingDateRequestService resolveOverlappingTrainingDateRequestService,
    ILogger<CohortDeletedWithPendingOverlappingTrainingDateEventHandler> logger)
    : IHandleMessages<CohortDeletedEvent>
{
    public async Task Handle(CohortDeletedEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("CohortDeletedEvent received for Cohort {CohortId}, with pending OverlappingTrainingDateRequest", message.CohortId);

        try
        {
            var cohort = await dbContext.Value.Cohorts
                .IgnoreQueryFilters()
                .Include(x => x.Apprenticeships)
                .ThenInclude(a => (a as Apprenticeship).OverlappingTrainingDateRequests)
                .Include(x => x.Apprenticeships)
                .ThenInclude(a => (a as DraftApprenticeship).OverlappingTrainingDateRequests)
                .Where(x => x.Id == message.CohortId)
                .FirstOrDefaultAsync();

            if (cohort == null)
            {
                logger.LogWarning("No cohort found for Id {CohortId}", message.CohortId);
                return;
            }

            var overlappingTrainingDateRequests = cohort.Apprenticeships
                .Where(apprenticeship => apprenticeship.OverlappingTrainingDateRequests != null)
                .SelectMany(apprenticeship => apprenticeship.OverlappingTrainingDateRequests)
                .ToList();

            if (overlappingTrainingDateRequests.Count == 0)
            {
                logger.LogWarning("No OverlappingTrainingDateRequests found for Cohort Id {cohortId}", message.CohortId);
                return;
            }

            foreach (var request in overlappingTrainingDateRequests)
            {
                if (request.Status == OverlappingTrainingDateRequestStatus.Pending)
                {
                    logger.LogInformation("Resolving OverlappingTrainingDateRequest {Id} for Cohort Id {CohortId}", request.Id, message.CohortId);

                    await resolveOverlappingTrainingDateRequestService.Resolve(
                        request.PreviousApprenticeshipId,
                        null,
                        OverlappingTrainingDateRequestResolutionType.CohortDeleted);
                }
                else
                {
                    logger.LogWarning("Unable to modify OverlappingTrainingDateRequest {Id} - status is already {Status}", request.Id, request.Status);
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error processing CohortWithPendingOverlappingTrainingDateRequestDeletedEvent");
            throw;
        }
    }
}