using SFA.DAS.Commitments.Events;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class CohortAssignedToEmployerEventHandlerForLegacyTaskCounter : IHandleMessages<CohortAssignedToEmployerEvent>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ILegacyTopicMessagePublisher _legacyTopicMessagePublisher;
        private readonly ILogger<CohortAssignedToEmployerEventHandlerForLegacyTaskCounter> _logger;

        public CohortAssignedToEmployerEventHandlerForLegacyTaskCounter(Lazy<ProviderCommitmentsDbContext> dbContext,  ILegacyTopicMessagePublisher legacyTopicMessagePublisher, ILogger<CohortAssignedToEmployerEventHandlerForLegacyTaskCounter> logger)
        {
            _dbContext = dbContext;
            _legacyTopicMessagePublisher = legacyTopicMessagePublisher;
            _logger = logger;
        }

        public async Task Handle(CohortAssignedToEmployerEvent message, IMessageHandlerContext context)
        {
            try
            {
                if (message.AssignedBy == Party.Provider)
                {
                    var cohort = await _dbContext.Value.Cohorts.SingleAsync(c => c.Id == message.CohortId); 
                    if(cohort.WithParty == Party.Employer)
                    {
                        await _legacyTopicMessagePublisher.PublishAsync(
                            new CohortApprovalRequestedByProvider(cohort.EmployerAccountId, cohort.ProviderId, cohort.Id));
                        _logger.LogInformation($"Published legacy event '{typeof(CohortApprovalRequestedByProvider)}' for Cohort {message.CohortId}");
                    }
                }
                _logger.LogInformation($"Handled event '{typeof(CohortAssignedToEmployerEvent)}' for Cohort {message.CohortId}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error publishing legacy Event '{typeof(CohortApprovalRequestedByProvider)}' for Cohort {message.CohortId}");
                throw;
            }
        }
    }
}
