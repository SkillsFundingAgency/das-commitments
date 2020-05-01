using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
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
            //go get the copr
            //get's it, but the Cohort part will be null because it's been deleted!!! :-O
            var changeOfPartyRequest = await _dbContext.Value.GetChangeOfPartyRequestAggregate(message.ChangeOfPartyRequestId, default);

            //if not in appropriate status, log error and quit happy
            if (changeOfPartyRequest.Status != ChangeOfPartyRequestStatus.Pending)
            {
                _logger.LogWarning($"Unable to modify ChangeOfPartyRequest {message.ChangeOfPartyRequestId} - status is already {changeOfPartyRequest.Status}");
                return;
            }

            //call either Withdraw() or Reject() based on status
            if(message.DeletedBy == changeOfPartyRequest.OriginatingParty)
            {
                changeOfPartyRequest.Withdraw(message.UserInfo);
            }
            else
            {
                changeOfPartyRequest.Reject(message.UserInfo);
            }

            await _dbContext.Value.SaveChangesAsync();
        }
    }
}
