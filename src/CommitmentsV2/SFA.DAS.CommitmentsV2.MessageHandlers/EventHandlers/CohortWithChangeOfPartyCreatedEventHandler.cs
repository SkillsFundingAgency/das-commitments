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
            _logger.LogInformation($"CohortWithChangeOfPartyCreatedEvent received for Cohort {message.CohortId}, ChangeOfPartyRequest {message.ChangeOfPartyRequestId}");

            try
            {
                var changeOfPartyRequest = await _dbContext.Value.GetChangeOfPartyRequestAggregate(message.ChangeOfPartyRequestId, default);
                var cohort = await _dbContext.Value.GetCohortAggregate(message.CohortId, default);

                if (changeOfPartyRequest.CohortId.HasValue)
                {
                    _logger.LogWarning($"ChangeOfPartyRequest {changeOfPartyRequest.Id} already has CohortId {changeOfPartyRequest.CohortId} - {nameof(CohortWithChangeOfPartyCreatedEvent)} with CohortId {message.CohortId} will be ignored");
                    return;
                }

                changeOfPartyRequest.SetCohort(cohort, message.UserInfo);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error processing CohortWithChangeOfPartyCreatedEvent", e);
                throw;
            }
        }
    }
}
