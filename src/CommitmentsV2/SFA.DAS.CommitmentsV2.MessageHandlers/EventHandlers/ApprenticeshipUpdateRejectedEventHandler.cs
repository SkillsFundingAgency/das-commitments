using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Commitments.Events;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class ApprenticeshipUpdateRejectedEventHandler : IHandleMessages<ApprenticeshipUpdateRejectedEvent>
    {
        private readonly ILegacyTopicMessagePublisher _legacyTopicMessagePublisher;
        private readonly ILogger<ApprenticeshipUpdateRejectedEventHandler> _logger;

        public ApprenticeshipUpdateRejectedEventHandler(ILegacyTopicMessagePublisher legacyTopicMessagePublisher, ILogger<ApprenticeshipUpdateRejectedEventHandler> logger)

        {
            _legacyTopicMessagePublisher = legacyTopicMessagePublisher;
            _logger = logger;
        }

        public async Task Handle(ApprenticeshipUpdateRejectedEvent message, IMessageHandlerContext context)
        {
            try
            {
                await _legacyTopicMessagePublisher.PublishAsync(new ApprenticeshipUpdateRejected
                {
                    AccountId = message.AccountId,
                    ProviderId = message.ProviderId,
                    ApprenticeshipId = message.ApprenticeshipId
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error when trying to publish ApprenticeshipUpdateAccepted");
                throw;
            }
        }
    }
}
