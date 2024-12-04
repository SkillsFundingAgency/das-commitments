using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers;

public class ProviderSendCohortCommandHandler(
    ILogger<ProviderSendCohortCommandHandler> logger,
    Lazy<ProviderCommitmentsDbContext> dbContext)
    : IHandleMessages<ProviderSendCohortCommand>
{
    public async Task Handle(ProviderSendCohortCommand message, IMessageHandlerContext context)
    {
        try
        {
            logger.LogInformation("Handling {TypeName} with MessageId '{MessageId}'", nameof(ProviderSendCohortCommand), context.MessageId);

            var cohort = await dbContext.Value.GetCohortAggregate(message.CohortId, default);

            if (cohort.WithParty != Party.Provider)
            {
                logger.LogWarning("Cohort {CohortId} has already been SentToOtherParty by the Provider", message.CohortId);
                return;
            }

            cohort.SendToOtherParty(Party.Provider, message.Message, message.UserInfo, DateTime.UtcNow);

            await dbContext.Value.SaveChangesAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error processing {TypeName}", nameof(ProviderSendCohortCommand));
            throw;
        }
    }
}