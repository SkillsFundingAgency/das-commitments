using System;
using System.Threading.Tasks;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class CohortWithChangeOfPartyCreatedEventHandler : IHandleMessages<CohortWithChangeOfPartyCreatedEvent>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public CohortWithChangeOfPartyCreatedEventHandler(Lazy<ProviderCommitmentsDbContext> dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(CohortWithChangeOfPartyCreatedEvent message, IMessageHandlerContext context)
        {
            var changeOfPartyRequest = await
                _dbContext.Value.GetChangeOfPartyRequestAggregate(message.ChangeOfPartyRequestId, default);

            var cohort = await _dbContext.Value.GetCohortAggregate(message.CohortId, default);

            changeOfPartyRequest.SetCohort(cohort, message.UserInfo);

            _dbContext.Value.SaveChanges();
        }
    }
}
