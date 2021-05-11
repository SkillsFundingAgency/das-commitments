using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Commitments.Events;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class ApprenticeshipUpdateCreatedEventHandler : IHandleMessages<ApprenticeshipUpdateCreatedEvent>
    {
        private readonly ILegacyTopicMessagePublisher _legacyTopicMessagePublisher;
        private readonly ILogger<ApprenticeshipUpdateCreatedEventHandler> _logger;

        public ApprenticeshipUpdateCreatedEventHandler(ILegacyTopicMessagePublisher legacyTopicMessagePublisher, ILogger<ApprenticeshipUpdateCreatedEventHandler> logger)
        {
            _legacyTopicMessagePublisher = legacyTopicMessagePublisher;
            _logger = logger;
        }

        public async Task Handle(ApprenticeshipUpdateCreatedEvent message, IMessageHandlerContext context)
        {
            _logger.LogInformation("Received ApprenticeshipUpdateCeatedEvent for ApprenticeshipId : " + message.ApprenticeshipId);
            try
            {
                await _legacyTopicMessagePublisher.PublishAsync(new ApprenticeshipUpdateCreated
                {
                    AccountId = message.AccountId,
                    ProviderId = message.ProviderId,
                    ApprenticeshipId = message.ApprenticeshipId
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error when trying to publish ApprenticeshipUpdateCreated");
                throw;
            }
        }
    }
}
