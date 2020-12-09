using NServiceBus;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers
{
    public class UpdateChangeOfPartyRequestCommandHandler : IHandleMessages<UpdateChangeOfPartyRequestCommand>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public UpdateChangeOfPartyRequestCommandHandler(Lazy<ProviderCommitmentsDbContext> dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(UpdateChangeOfPartyRequestCommand message, IMessageHandlerContext context)
        {
            var cohort = await _dbContext.Value.GetCohortAggregate(message.CohortId, default);

            var changeOfPartyRequest = await _dbContext.Value.GetChangeOfPartyRequestAggregate(cohort.ChangeOfPartyRequestId.Value, default);

            var draftApprenticeship = cohort.DraftApprenticeships.FirstOrDefault();
            
            changeOfPartyRequest.UpdateChangeOfPartyRequest(draftApprenticeship, cohort.EmployerAccountId, cohort.ProviderId, message.UserInfo, cohort.WithParty);
        }
    }
}
