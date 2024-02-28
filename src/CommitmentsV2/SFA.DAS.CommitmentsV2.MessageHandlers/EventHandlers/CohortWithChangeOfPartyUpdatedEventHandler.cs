using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Models;
using Microsoft.Azure.ServiceBus;

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
            _logger.LogInformation($"CohortWithChangeOfPartyUpdatedEvent received for Cohort : {message.CohortId}");

            try
            {
                var cohort = await _dbContext.Value.GetCohortAggregate(message.CohortId, default);

                var changeOfPartyRequest = await _dbContext.Value.GetChangeOfPartyRequestAggregate(cohort.ChangeOfPartyRequestId.Value, default);

                if (changeOfPartyRequest.ChangeOfPartyType == ChangeOfPartyRequestType.ChangeProvider)
                {
                    var draftApprenticeship = cohort.DraftApprenticeships.FirstOrDefault();

                    changeOfPartyRequest.UpdateChangeOfPartyRequest(draftApprenticeship, cohort.EmployerAccountId,
                        cohort.ProviderId, message.UserInfo, cohort.WithParty);
                }
            }
            catch (InvalidOperationException e)
            {
                _logger.LogError(e, $"InvalidOperationException processing CohortWithChangeOfPartyUpdatedEvent", e);
                if (!e.Message.EndsWith("is approved by all parties and can't be modified"))
                {
                    throw;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error processing CohortWithChangeOfPartyUpdatedEvent", e);
                throw;
            }
        }
    }
}
