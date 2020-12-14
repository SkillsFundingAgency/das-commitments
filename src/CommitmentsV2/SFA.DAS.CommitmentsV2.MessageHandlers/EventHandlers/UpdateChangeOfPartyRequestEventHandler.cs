using NServiceBus;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class UpdateChangeOfPartyRequestEventHandler : IHandleMessages<UpdateChangeOfPartyRequestEvent>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public UpdateChangeOfPartyRequestEventHandler(Lazy<ProviderCommitmentsDbContext> dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(UpdateChangeOfPartyRequestEvent message, IMessageHandlerContext context)
        {
            var cohort = await _dbContext.Value.GetCohortAggregate(message.CohortId, default);

            var changeOfPartyRequest = await _dbContext.Value.GetChangeOfPartyRequestAggregate(cohort.ChangeOfPartyRequestId.Value, default);

            if (changeOfPartyRequest.ChangeOfPartyType == ChangeOfPartyRequestType.ChangeProvider)
            {
                var draftApprenticeship = cohort.DraftApprenticeships.FirstOrDefault();

                changeOfPartyRequest.UpdateChangeOfPartyRequest(draftApprenticeship, cohort.EmployerAccountId, cohort.ProviderId, message.UserInfo, cohort.WithParty);
            }
        }
    }
}
