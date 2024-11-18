using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers;

public class ProviderApproveCohortCommandHandler(
    ILogger<ProviderApproveCohortCommandHandler> logger,
    Lazy<ProviderCommitmentsDbContext> dbContext,
    IEmailOptionalService emailService)
    : IHandleMessages<ProviderApproveCohortCommand>
{
    public async Task Handle(ProviderApproveCohortCommand message, IMessageHandlerContext context)
    {
        try
        {
            logger.LogInformation("Handling {TypeName} with MessageId '{MessageId}'", nameof(ProviderApproveCohortCommand), context.MessageId);

            var cohort = await dbContext.Value.GetCohortAggregate(message.CohortId, default);
            var apprenticeEmailIsRequired = emailService.ApprenticeEmailIsRequiredFor(cohort.EmployerAccountId, cohort.ProviderId);

            if (cohort.Approvals.HasFlag(Party.Provider))
            {
                logger.LogWarning($"Cohort {message.CohortId} has already been approved by the Provider");
                return;
            }

            cohort.Approve(Party.Provider, message.Message, message.UserInfo, DateTime.UtcNow, apprenticeEmailIsRequired);

            await dbContext.Value.SaveChangesAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error processing {TypeName}", nameof(ProviderApproveCohortCommand));
            throw;
        }
    }
}