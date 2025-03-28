using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers;

public class EmployerSendCohortCommandHandler(
    ILogger<EmployerSendCohortCommandHandler> logger,
    Lazy<ProviderCommitmentsDbContext> dbContext)
    : IHandleMessages<EmployerSendCohortCommand>
{
    public async Task Handle(EmployerSendCohortCommand message, IMessageHandlerContext context)
    {
        try
        {
            logger.LogInformation("Handling {TypeName} with MessageId '{MessageId}'", nameof(EmployerSendCohortCommand), context.MessageId);

            var cohort = await dbContext.Value.GetCohortAggregate(message.CohortId, default);

            if (cohort.WithParty != Party.Employer)
            {
                logger.LogWarning("Cohort {CohortId} has already been SentToOtherParty by the Employer", message.CohortId);
                return;
            }

            cohort.SendToOtherParty(Party.Employer, message.Message, message.UserInfo, DateTime.UtcNow);

            await dbContext.Value.SaveChangesAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error processing {TypeName}", nameof(EmployerSendCohortCommand));
            throw;
        }
    }
}