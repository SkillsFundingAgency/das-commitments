using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class CohortWithChangeOfPartyDeletedEventHandler(Lazy<ProviderCommitmentsDbContext> dbContext, ILogger<CohortWithChangeOfPartyDeletedEventHandler> logger)
    : IHandleMessages<CohortWithChangeOfPartyDeletedEvent>
{
    public async Task Handle(CohortWithChangeOfPartyDeletedEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("CohortWithChangeOfPartyDeletedEvent received for Cohort {Id}, ChangeOfPartyRequest {ChangeOfPartyRequestId}", message.CohortId, message.ChangeOfPartyRequestId);

        try
        {
            var changeOfPartyRequest =
                await dbContext.Value.GetChangeOfPartyRequestAggregate(message.ChangeOfPartyRequestId, default);

            if (changeOfPartyRequest.Status != ChangeOfPartyRequestStatus.Pending)
            {
                logger.LogWarning("Unable to modify ChangeOfPartyRequest {ChangeOfPartyRequestId} - status is already {Status}", message.ChangeOfPartyRequestId, changeOfPartyRequest.Status);
                return;
            }

            if (message.DeletedBy == changeOfPartyRequest.OriginatingParty)
            {
                changeOfPartyRequest.Withdraw(message.DeletedBy, message.UserInfo);
            }
            else
            {
                changeOfPartyRequest.Reject(message.DeletedBy, message.UserInfo);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error processing CohortWithChangeOfPartyDeletedEvent");
            throw;
        }
    }
}