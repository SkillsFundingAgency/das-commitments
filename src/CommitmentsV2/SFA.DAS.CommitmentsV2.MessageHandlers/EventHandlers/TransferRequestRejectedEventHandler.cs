using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Commitments.Events;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class TransferRequestRejectedEventHandler(Lazy<ProviderCommitmentsDbContext> dbContext, ILegacyTopicMessagePublisher legacyTopicMessagePublisher, ILogger<TransferRequestRejectedEvent> logger)
    : IHandleMessages<TransferRequestRejectedEvent>
{
    public async Task Handle(TransferRequestRejectedEvent message, IMessageHandlerContext context)
    {
        try
        {
            logger.LogInformation("TransferRequestRejectedEvent received for CohortId : {CohortId}, TransferRequestId : {TransferRequestId}", message.CohortId, message.TransferRequestId);

            var db = dbContext.Value;

            var cohort = await dbContext.Value.Cohorts.SingleAsync(c => c.Id == message.CohortId);
            cohort.RejectTransferRequest(message.UserInfo);

            var transferRequest = await db.TransferRequests.SingleAsync(x => x.Id == message.TransferRequestId);

            if (transferRequest.AutoApproval)
            {
                logger.LogInformation("AutoApproval set to true - not publishing CohortRejectedByTransferSender");

                return;
            }

            // Publish legacy event so Tasks can decrement it's counter
            await legacyTopicMessagePublisher.PublishAsync(new CohortRejectedByTransferSender(
                message.TransferRequestId,
                cohort.EmployerAccountId,
                cohort.Id,
                cohort.TransferSenderId.Value,
                message.UserInfo.UserDisplayName,
                message.UserInfo.UserEmail));

            logger.LogInformation("Cohort {CohortId} returned to Employer, after TransferRequest {TransferRequestId} was rejected", message.CohortId, message.TransferRequestId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error when trying to reject Cohort {CohortId} for TransferRequest {TransferRequestId}", message.CohortId, message.TransferRequestId);
            throw;
        }
    }
}