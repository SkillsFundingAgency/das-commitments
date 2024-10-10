using SFA.DAS.Commitments.Events;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class CohortAssignedToEmployerEventHandlerForLegacyTaskCounter(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ILegacyTopicMessagePublisher legacyTopicMessagePublisher,
    ILogger<CohortAssignedToEmployerEventHandlerForLegacyTaskCounter> logger)
    : IHandleMessages<CohortAssignedToEmployerEvent>
{
    public async Task Handle(CohortAssignedToEmployerEvent message, IMessageHandlerContext context)
    {
        try
        {
            if (message.AssignedBy == Party.Provider)
            {
                var cohort = await dbContext.Value.Cohorts.SingleAsync(c => c.Id == message.CohortId);
                if (cohort.WithParty == Party.Employer)
                {
                    await legacyTopicMessagePublisher.PublishAsync(new CohortApprovalRequestedByProvider(cohort.EmployerAccountId, cohort.ProviderId, cohort.Id));

                    logger.LogInformation("Published legacy event '{TypeName}' for Cohort {CohortId}", typeof(CohortApprovalRequestedByProvider), message.CohortId);
                }
            }

            logger.LogInformation("Handled event '{TypeName}' for Cohort {CohortId}", typeof(CohortApprovalRequestedByProvider), message.CohortId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error publishing legacy Event '{TypeName}' for Cohort {CohortId}", typeof(CohortApprovalRequestedByProvider), message.CohortId);
            throw;
        }
    }
}