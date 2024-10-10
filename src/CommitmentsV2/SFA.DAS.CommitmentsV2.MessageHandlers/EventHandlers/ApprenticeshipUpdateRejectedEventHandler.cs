using SFA.DAS.Commitments.Events;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class ApprenticeshipUpdateRejectedEventHandler(ILegacyTopicMessagePublisher legacyTopicMessagePublisher, ILogger<ApprenticeshipUpdateRejectedEventHandler> logger)
    : IHandleMessages<ApprenticeshipUpdateRejectedEvent>
{
    public async Task Handle(ApprenticeshipUpdateRejectedEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("Received ApprenticeshipUpdateRejectedEvent for apprenticeshipId : {Id}.", message.ApprenticeshipId);
        try
        {
            await legacyTopicMessagePublisher.PublishAsync(new ApprenticeshipUpdateRejected
            {
                AccountId = message.AccountId,
                ProviderId = message.ProviderId,
                ApprenticeshipId = message.ApprenticeshipId
            });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error when trying to publish ApprenticeshipUpdateRejected");
            throw;
        }
    }
}