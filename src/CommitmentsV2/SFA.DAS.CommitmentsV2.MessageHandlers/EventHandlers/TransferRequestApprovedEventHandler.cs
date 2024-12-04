using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class TransferRequestApprovedEventHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ILogger<TransferRequestApprovedEventHandler> logger)
    : IHandleMessages<TransferRequestApprovedEvent>
{
    public async Task Handle(TransferRequestApprovedEvent message, IMessageHandlerContext context)
    {
        try
        {
            logger.LogInformation($"TransferRequestApprovedEvent received for CohortId : {message.CohortId}, TransferRequestId : {message.TransferRequestId}");

            var db = dbContext.Value;

            var cohort = await db.Cohorts.Include(c => c.Apprenticeships).SingleAsync(c => c.Id == message.CohortId);
            cohort.Approve(Party.TransferSender, null, message.UserInfo, message.ApprovedOn);
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Error when trying to approve Cohort {message.CohortId} for TransferRequest {message.TransferRequestId}");
            throw;
        }
    }
}
