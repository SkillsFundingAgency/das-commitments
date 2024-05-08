using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers
{
    public class ProviderSendCohortCommandHandler : IHandleMessages<ProviderSendCohortCommand>
    {
        private readonly ILogger<ProviderSendCohortCommandHandler> _logger;
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public ProviderSendCohortCommandHandler(
            ILogger<ProviderSendCohortCommandHandler> logger,
            Lazy<ProviderCommitmentsDbContext> dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task Handle(ProviderSendCohortCommand message, IMessageHandlerContext context)
        {
            try
            {
                _logger.LogInformation($"Handling {nameof(ProviderSendCohortCommand)} with MessageId '{context.MessageId}'");

                var cohort = await _dbContext.Value.GetCohortAggregate(message.CohortId, default);

                if (cohort.WithParty != Party.Provider)
                {
                    _logger.LogWarning($"Cohort {message.CohortId} has already been SentToOtherParty by the Provider");
                    return;
                }

                cohort.SendToOtherParty(Party.Provider, message.Message, message.UserInfo, DateTime.UtcNow);

                await _dbContext.Value.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError($"Error processing {nameof(ProviderSendCohortCommand)}", e);
                throw;
            }
        }
    }
}
