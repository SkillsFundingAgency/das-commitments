using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class ApprenticeshipWithChangeOfPartyCreatedEventHandler : IHandleMessages<ApprenticeshipWithChangeOfPartyCreatedEvent>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ILogger<ApprenticeshipWithChangeOfPartyCreatedEventHandler> _logger;

        public ApprenticeshipWithChangeOfPartyCreatedEventHandler(Lazy<ProviderCommitmentsDbContext> dbContext, ILogger<ApprenticeshipWithChangeOfPartyCreatedEventHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Handle(ApprenticeshipWithChangeOfPartyCreatedEvent message, IMessageHandlerContext context)
        {
            _logger.LogInformation($"ApprenticeshipWithChangeOfPartyCreatedEvent received for Apprenticeship {message.ApprenticeshipId}, ChangeOfPartyRequest {message.ChangeOfPartyRequestId}");

            try
            {
                var changeOfPartyRequest = await _dbContext.Value.GetChangeOfPartyRequestAggregate(message.ChangeOfPartyRequestId, default);
                var apprenticeship = await _dbContext.Value.GetApprenticeshipAggregate(message.ApprenticeshipId, default);

                if (changeOfPartyRequest.NewApprenticeshipId.HasValue)
                {
                    _logger.LogWarning($"ChangeOfPartyRequest {changeOfPartyRequest.Id} already has NewApprenticeshipId {changeOfPartyRequest.CohortId} - {nameof(ApprenticeshipWithChangeOfPartyCreatedEvent)} with new ApprenticeshipId {message.ApprenticeshipId} will be ignored");
                    return;
                }

                changeOfPartyRequest.SetNewApprenticeship(apprenticeship, message.UserInfo, message.LastApprovedBy);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error processing ApprenticeshipWithChangeOfPartyCreatedEvent", e);
                throw;
            }
        }
    }
}
