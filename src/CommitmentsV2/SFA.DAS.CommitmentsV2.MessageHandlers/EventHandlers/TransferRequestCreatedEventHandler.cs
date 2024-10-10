using SFA.DAS.Commitments.Events;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class TransferRequestCreatedEventHandler(ILegacyTopicMessagePublisher legacyTopicMessagePublisher, ILogger<TransferRequestCreatedEvent> logger, Lazy<ProviderCommitmentsDbContext> dbContext)
    : IHandleMessages<TransferRequestCreatedEvent>
{
    public async Task Handle(TransferRequestCreatedEvent message, IMessageHandlerContext context)
    {
        try
        {
            var db = dbContext.Value;
            var transferRequest = await db.TransferRequests.Include(c => c.Cohort)
                .SingleAsync(x => x.Id == message.TransferRequestId);

            if (transferRequest.AutoApproval)
            {
                logger.LogInformation("AutoApproval set to true - not publishing CohortApprovalByTransferSenderRequested");

                return;
            }

            logger.LogInformation("AutoApproval set to false - publishing CohortApprovalByTransferSenderRequested");

            await legacyTopicMessagePublisher.PublishAsync(new CohortApprovalByTransferSenderRequested
            {
                TransferRequestId = message.TransferRequestId,
                ReceivingEmployerAccountId = transferRequest.Cohort.EmployerAccountId,
                SendingEmployerAccountId = transferRequest.Cohort.TransferSenderId.Value,
                TransferCost = transferRequest.Cost,
                CommitmentId = transferRequest.CommitmentId
            });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error when trying to publish CohortApprovalByTransferSenderRequested");
            throw;
        }
    }
}