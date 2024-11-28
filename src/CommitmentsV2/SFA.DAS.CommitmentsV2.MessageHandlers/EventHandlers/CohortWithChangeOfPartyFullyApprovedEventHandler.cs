using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class CohortWithChangeOfPartyFullyApprovedEventHandler(Lazy<ProviderCommitmentsDbContext> dbContext, ILogger<CohortWithChangeOfPartyFullyApprovedEventHandler> logger)
    : IHandleMessages<CohortWithChangeOfPartyFullyApprovedEvent>
{
    public async Task Handle(CohortWithChangeOfPartyFullyApprovedEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("CohortWithChangeOfPartyFullyApprovedEvent received for Cohort {CohortId}, ChangeOfPartyRequest {ChangeOfPartyRequestId}", message.CohortId, message.ChangeOfPartyRequestId);

        try
        {
            var changeOfPartyRequest = await dbContext.Value.GetChangeOfPartyRequestAggregateSafely(message.ChangeOfPartyRequestId, default);

            if (changeOfPartyRequest == null)
            {
                logger.LogInformation("CohortWithChangeOfPartyFullyApprovedEvent received for Cohort {CohortId}, ChangeOfPartyRequest {ChangeOfPartyRequestId}", message.CohortId, message.ChangeOfPartyRequestId);
                return;
            }

            if (changeOfPartyRequest.Status != ChangeOfPartyRequestStatus.Pending)
            {
                logger.LogWarning("Unable to Approve ChangeOfPartyRequest {ChangeOfPartyRequestId} - status is already {Status}", message.ChangeOfPartyRequestId, changeOfPartyRequest.Status);
                return;
            }

            changeOfPartyRequest.Approve(message.ApprovedBy, message.UserInfo);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error processing CohortWithChangeOfPartyFullyApprovedEvent");
            throw;
        }
    }
}