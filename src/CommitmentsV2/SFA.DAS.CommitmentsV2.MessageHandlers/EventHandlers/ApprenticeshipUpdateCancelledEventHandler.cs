using SFA.DAS.Commitments.Events;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class ApprenticeshipUpdateCancelledEventHandler(ILegacyTopicMessagePublisher legacyTopicMessagePublisher, ILogger<ApprenticeshipUpdateCancelledEventHandler> logger)
    : IHandleMessages<ApprenticeshipUpdateCancelledEvent>
{
    public async Task Handle(ApprenticeshipUpdateCancelledEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("Received ApprenticeshipUpdateCancelledEvent for apprenticeshipId: {ApprenticeshipId}", message.ApprenticeshipId);
        
        try
        {
            await legacyTopicMessagePublisher.PublishAsync(new ApprenticeshipUpdateCancelled
            {
                AccountId = message.AccountId,
                ProviderId = message.ProviderId,
                ApprenticeshipId = message.ApprenticeshipId
            });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error when trying to publish ApprenticeshipUpdateCancelled");
            throw;
        }
    }
}