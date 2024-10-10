using SFA.DAS.Commitments.Events;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class CohortDeletedEventHandler(ILegacyTopicMessagePublisher legacyTopicMessagePublisher, ILogger<CohortDeletedEventHandler> logger)
    : IHandleMessages<CohortDeletedEvent>
{
    public async Task Handle(CohortDeletedEvent message, IMessageHandlerContext context)
    {
        try
        {
            if (message.ApprovedBy.HasFlag(Party.Provider))
            {
                await legacyTopicMessagePublisher.PublishAsync(new ProviderCohortApprovalUndoneByEmployerUpdate(message.AccountId, message.ProviderId, message.CohortId));
                logger.LogInformation("Sent message '{TypeName}' for commitment {CohortId}", nameof(ProviderCohortApprovalUndoneByEmployerUpdate), message.CohortId);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error when trying to publish {TypeName}", nameof(ProviderCohortApprovalUndoneByEmployerUpdate));
            throw;
        }
    }
}