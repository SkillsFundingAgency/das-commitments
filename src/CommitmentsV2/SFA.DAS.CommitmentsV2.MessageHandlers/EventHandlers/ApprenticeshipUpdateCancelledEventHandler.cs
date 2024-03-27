using SFA.DAS.Commitments.Events;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class ApprenticeshipUpdateCancelledEventHandler : IHandleMessages<ApprenticeshipUpdateCancelledEvent>
    {
        private readonly ILegacyTopicMessagePublisher _legacyTopicMessagePublisher;
        private readonly ILogger<ApprenticeshipUpdateCancelledEventHandler> _logger;

        public ApprenticeshipUpdateCancelledEventHandler(ILegacyTopicMessagePublisher legacyTopicMessagePublisher, ILogger<ApprenticeshipUpdateCancelledEventHandler> logger)

        {
            _legacyTopicMessagePublisher = legacyTopicMessagePublisher;
            _logger = logger;
        }

        public async Task Handle(ApprenticeshipUpdateCancelledEvent message, IMessageHandlerContext context)
        {
            _logger.LogInformation("Received ApprenticeshipUpdateCancelledEvent for apprenticeshipId : " + message.ApprenticeshipId);
            try
            {
                await _legacyTopicMessagePublisher.PublishAsync(new ApprenticeshipUpdateCancelled
                {
                    AccountId = message.AccountId,
                    ProviderId = message.ProviderId,
                    ApprenticeshipId = message.ApprenticeshipId
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error when trying to publish ApprenticeshipUpdateCancelled");
                throw;
            }
        }
    }
}
