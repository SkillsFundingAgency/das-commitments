using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class CohortWithChangeOfPartyUpdatedEventHandler : IHandleMessages<CohortWithChangeOfPartyUpdatedEvent>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ILogger<CohortWithChangeOfPartyUpdatedEventHandler> _logger;

        public CohortWithChangeOfPartyUpdatedEventHandler(Lazy<ProviderCommitmentsDbContext> dbContext, ILogger<CohortWithChangeOfPartyUpdatedEventHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Handle(CohortWithChangeOfPartyUpdatedEvent message, IMessageHandlerContext context)
        {
            _logger.LogInformation("CohortWithChangeOfPartyUpdatedEvent received for Cohort : {CohortId}", message.CohortId);

            try
            {
                var cohort = await _dbContext.Value.GetCohortAggregateSafely(message.CohortId, default);

                if (cohort == null)
                {
                    _logger.LogInformation("Cohort {Cohort} not found, CohortWithChangeOfPartyUpdatedEvent is not needed", message.CohortId);
                    return;
                }

                if (cohort.IsApprovedByAllParties)
                {
                    _logger.LogInformation("Cohort {Cohort} is fully approved, CohortWithChangeOfPartyUpdatedEvent is not needed", message.CohortId);
                    return;
                }

                var changeOfPartyRequest = await _dbContext.Value.GetChangeOfPartyRequestAggregateSafely(cohort.ChangeOfPartyRequestId.Value, default);

                if (changeOfPartyRequest == null)
                {
                    _logger.LogInformation("ChangeOfParty request {ChangeOfPartyRequestId} not found, CohortWithChangeOfPartyUpdatedEvent is not needed", cohort.ChangeOfPartyRequestId);
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
                _logger.LogError(e, "Error processing CohortWithChangeOfPartyUpdatedEvent", e);
                throw;
            }
        }
    }
}
