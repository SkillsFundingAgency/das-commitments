using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class CohortWithChangeOfPartyCreatedEventHandler : IHandleMessages<CohortWithChangeOfPartyCreatedEvent>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ILogger<CohortWithChangeOfPartyCreatedEventHandler> _logger;

        public CohortWithChangeOfPartyCreatedEventHandler(Lazy<ProviderCommitmentsDbContext> dbContext,
            ILogger<CohortWithChangeOfPartyCreatedEventHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Handle(CohortWithChangeOfPartyCreatedEvent message, IMessageHandlerContext context)
        {
            _logger.LogInformation("CohortWithChangeOfPartyCreatedEvent received for Cohort {CohortId}, ChangeOfPartyRequest {ChangeOfPartyRequestId}", message.CohortId, message.ChangeOfPartyRequestId);

            try
            {
                var changeOfPartyRequest = await _dbContext.Value.GetChangeOfPartyRequestAggregateSafely(message.ChangeOfPartyRequestId, default);
                if (changeOfPartyRequest == null)
                {
                    _logger.LogInformation("ChangeOfPartyRequest {ChangeOfPartyRequestId} not found", message.ChangeOfPartyRequestId);
                    return;
                }

                var cohort = await _dbContext.Value.GetCohortAggregateSafely(message.CohortId, default);
                if (cohort == null)
                {
                    _logger.LogInformation("Cohort {CohortId} not found", message.CohortId);
                    return;
                }

                if (changeOfPartyRequest.CohortId.HasValue)
                {
                    _logger.LogWarning("ChangeOfPartyRequest {changeOfPartyRequestId} already has CohortId {changeOfPartyRequestCohortId} - {Event} with CohortId {messageCohortId} will be ignored", 
                        changeOfPartyRequest.Id, changeOfPartyRequest.CohortId, nameof(CohortWithChangeOfPartyCreatedEvent), message.CohortId);
                    return;
                }

                changeOfPartyRequest.SetCohort(cohort, message.UserInfo);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error processing CohortWithChangeOfPartyCreatedEvent", e);
                throw;
            }
        }
    }
}
