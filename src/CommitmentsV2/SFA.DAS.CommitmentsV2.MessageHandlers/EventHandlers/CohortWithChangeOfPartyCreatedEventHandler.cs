using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class CohortWithChangeOfPartyCreatedEventHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ILogger<CohortWithChangeOfPartyCreatedEventHandler> logger)
    : IHandleMessages<CohortWithChangeOfPartyCreatedEvent>
{
    public async Task Handle(CohortWithChangeOfPartyCreatedEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("CohortWithChangeOfPartyCreatedEvent received for Cohort {CohortId}, ChangeOfPartyRequest {ChangeOfPartyRequestId}", message.CohortId, message.ChangeOfPartyRequestId);

        try
        {
            var changeOfPartyRequest = await dbContext.Value.GetChangeOfPartyRequestAggregateSafely(message.ChangeOfPartyRequestId, default);
            if (changeOfPartyRequest == null)
            {
                logger.LogInformation("ChangeOfPartyRequest {ChangeOfPartyRequestId} not found", message.ChangeOfPartyRequestId);
                return;
            }

            var cohort = await dbContext.Value.GetCohortAggregateSafely(message.CohortId, default);
            if (cohort == null)
            {
                logger.LogInformation("Cohort {CohortId} not found", message.CohortId);
                return;
            }

            if (changeOfPartyRequest.CohortId.HasValue)
            {
                logger.LogWarning("ChangeOfPartyRequest {ChangeOfPartyRequestId} already has CohortId {ChangeOfPartyRequestCohortId} - {Event} with CohortId {MessageCohortId} will be ignored", 
                    changeOfPartyRequest.Id,
                    changeOfPartyRequest.CohortId,
                    nameof(CohortWithChangeOfPartyCreatedEvent),
                    message.CohortId);
                
                return;
            }

            changeOfPartyRequest.SetCohort(cohort, message.UserInfo);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error processing CohortWithChangeOfPartyCreatedEvent");
            throw;
        }
    }
}