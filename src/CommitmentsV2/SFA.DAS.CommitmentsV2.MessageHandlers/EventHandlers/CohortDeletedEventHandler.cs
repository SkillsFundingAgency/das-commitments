using SFA.DAS.Commitments.Events;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class CohortDeletedEventHandler : IHandleMessages<CohortDeletedEvent>
    {
        private readonly ILegacyTopicMessagePublisher _legacyTopicMessagePublisher;
        private readonly ILogger<CohortDeletedEventHandler> _logger;

        public CohortDeletedEventHandler(ILegacyTopicMessagePublisher legacyTopicMessagePublisher, ILogger<CohortDeletedEventHandler> logger)
        {
            _legacyTopicMessagePublisher = legacyTopicMessagePublisher;
            _logger = logger;
        }

        public async Task Handle(CohortDeletedEvent message, IMessageHandlerContext context)
        {
            try
            {
                if (message.ApprovedBy.HasFlag(Party.Provider))
                {
                    await _legacyTopicMessagePublisher.PublishAsync(
                        new ProviderCohortApprovalUndoneByEmployerUpdate(message.AccountId, message.ProviderId,
                            message.CohortId));
                    _logger.LogInformation(
                        $"Sent message '{nameof(ProviderCohortApprovalUndoneByEmployerUpdate)}' for commitment {message.CohortId}");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error when trying to publish {nameof(ProviderCohortApprovalUndoneByEmployerUpdate)}");
                throw;
            }
        }
    }
}