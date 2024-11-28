using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class CohortWithChangeOfPartyUpdatedEventHandler(Lazy<ProviderCommitmentsDbContext> dbContext, ILogger<CohortWithChangeOfPartyUpdatedEventHandler> logger)
    : IHandleMessages<CohortWithChangeOfPartyUpdatedEvent>
{
    public async Task Handle(CohortWithChangeOfPartyUpdatedEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("CohortWithChangeOfPartyUpdatedEvent received for Cohort : {CohortId}", message.CohortId);

        try
        {
            var cohort = await dbContext.Value.GetCohortAggregateSafely(message.CohortId, default);

            if (cohort == null)
            {
                logger.LogInformation("Cohort {Cohort} not found, CohortWithChangeOfPartyUpdatedEvent is not needed", message.CohortId);
                return;
            }

            if (cohort.IsApprovedByAllParties)
            {
                logger.LogInformation("Cohort {Cohort} is fully approved, CohortWithChangeOfPartyUpdatedEvent is not needed", message.CohortId);
                return;
            }

            var changeOfPartyRequest = await dbContext.Value.GetChangeOfPartyRequestAggregateSafely(cohort.ChangeOfPartyRequestId.Value, default);

            if (changeOfPartyRequest == null)
            {
                logger.LogInformation("ChangeOfParty request {ChangeOfPartyRequestId} not found, CohortWithChangeOfPartyUpdatedEvent is not needed", cohort.ChangeOfPartyRequestId);
                return;
            }

            if (changeOfPartyRequest.ChangeOfPartyType == ChangeOfPartyRequestType.ChangeProvider)
            {
                var draftApprenticeship = cohort.DraftApprenticeships.FirstOrDefault();

                changeOfPartyRequest.UpdateChangeOfPartyRequest(draftApprenticeship, cohort.EmployerAccountId,
                    cohort.ProviderId, message.UserInfo, cohort.WithParty);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error processing CohortWithChangeOfPartyUpdatedEvent");
            throw;
        }
    }
}