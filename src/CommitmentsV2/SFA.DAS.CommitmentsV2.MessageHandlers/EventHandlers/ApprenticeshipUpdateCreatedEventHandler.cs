using SFA.DAS.Commitments.Events;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class ApprenticeshipUpdateCreatedEventHandler(
    ILegacyTopicMessagePublisher legacyTopicMessagePublisher,
    ILogger<ApprenticeshipUpdateCreatedEventHandler> logger)
    : IHandleMessages<ApprenticeshipUpdateCreatedEvent>
{
    public async Task Handle(ApprenticeshipUpdateCreatedEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("Received ApprenticeshipUpdateCreatedEvent for ApprenticeshipId: {ApprenticeshipId}", message.ApprenticeshipId);
        try
        {
            await legacyTopicMessagePublisher.PublishAsync(new ApprenticeshipUpdateCreated
            {
                AccountId = message.AccountId,
                ProviderId = message.ProviderId,
                ApprenticeshipId = message.ApprenticeshipId
            });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error when trying to publish ApprenticeshipUpdateCreated");
            throw;
        }
    }
}