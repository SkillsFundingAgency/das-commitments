using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class CohortWithChangeOfPartyDeletedEventHandler :IHandleMessages<CohortWithChangeOfPartyDeletedEvent>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ILogger<CohortWithChangeOfPartyDeletedEventHandler> _logger;

        public CohortWithChangeOfPartyDeletedEventHandler(Lazy<ProviderCommitmentsDbContext> dbContext, ILogger<CohortWithChangeOfPartyDeletedEventHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Handle(CohortWithChangeOfPartyDeletedEvent message, IMessageHandlerContext context)
        {
            _logger.LogInformation($"CohortWithChangeOfPartyDeletedEvent received for Cohort {message.CohortId}, ChangeOfPartyRequest {message.ChangeOfPartyRequestId}");

            try
            {
                var changeOfPartyRequest =
                    await _dbContext.Value.GetChangeOfPartyRequestAggregate(message.ChangeOfPartyRequestId, default);

                if (changeOfPartyRequest.Status != ChangeOfPartyRequestStatus.Pending)
                {
                    _logger.LogWarning(
                        $"Unable to modify ChangeOfPartyRequest {message.ChangeOfPartyRequestId} - status is already {changeOfPartyRequest.Status}");
                    return;
                }

                if (message.DeletedBy == changeOfPartyRequest.OriginatingParty)
                {
                    changeOfPartyRequest.Withdraw(message.DeletedBy, message.UserInfo);
                }
                else
                {
                    changeOfPartyRequest.Reject(message.DeletedBy, message.UserInfo);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error processing CohortWithChangeOfPartyDeletedEvent", e);
                throw;
            }
        }
    }
}
