using SFA.DAS.Commitments.Events;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class TransferRequestApprovedEventHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ILegacyTopicMessagePublisher legacyTopicMessagePublisher,
    ILogger<TransferRequestApprovedEvent> logger)
    : IHandleMessages<TransferRequestApprovedEvent>
{
    public async Task Handle(TransferRequestApprovedEvent message, IMessageHandlerContext context)
    {
        try
        {
            logger.LogInformation("TransferRequestApprovedEvent received for CohortId : {CohortId}, TransferRequestId : {TransferRequestId}", message.CohortId, message.TransferRequestId);

            var db = dbContext.Value;

            var cohort = await db.Cohorts.Include(c => c.Apprenticeships).SingleAsync(c => c.Id == message.CohortId);
            cohort.Approve(Party.TransferSender, null, message.UserInfo, message.ApprovedOn);

            var transferRequest = await db.TransferRequests.SingleAsync(x => x.Id == message.TransferRequestId);

            if (transferRequest.AutoApproval)
            {
                logger.LogInformation("AutoApproval set to true - not publishing CohortApprovedByTransferSender");

                return;
            }

            logger.LogInformation("AutoApproval set to false - publishing CohortApprovedByTransferSender");

            // Publish legacy event so Tasks can decrement it's counter
            await legacyTopicMessagePublisher.PublishAsync(new CohortApprovedByTransferSender(message.TransferRequestId,
                cohort.EmployerAccountId,
                cohort.Id,
                cohort.TransferSenderId.Value,
                message.UserInfo.UserDisplayName,
                message.UserInfo.UserEmail));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error when trying to approve Cohort {CohortId} for TransferRequest {TransferRequestId}", message.CohortId, message.TransferRequestId);
            throw;
        }
    }
}