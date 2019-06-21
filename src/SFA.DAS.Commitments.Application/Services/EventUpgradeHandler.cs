using NServiceBus;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Events;
using SFA.DAS.NLog.Logger;
using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Application.Services
{
    public class EventUpgradeHandler :
        IEventUpgradeHandler<Events.CohortApprovalRequestedByProvider>,
        IEventUpgradeHandler<Events.CohortApprovedByEmployer>
    {
        private readonly IEndpointInstance _endpointInstance;
        private readonly ILog _logger;

        public EventUpgradeHandler(IEndpointInstance endpointInstance, ILog logger)
        {
            _endpointInstance = endpointInstance;
            _logger = logger;
        }

        public Task Execute(Events.CohortApprovalRequestedByProvider @event)
        {
            _logger.Debug($"Upgrading {nameof(Events.CohortApprovalRequestedByProvider)} to publish with NServiceBus");
            return _endpointInstance.Publish(new CommitmentsV2.Messages.Events.CohortApprovalRequestedByProvider()
            {
                AccountId = @event.AccountId,
                ProviderId = @event.ProviderId,
                CommitmentId = @event.CommitmentId
            });
        }

        public Task Execute(CohortApprovedByEmployer @event)
        {
            _logger.Debug($"Upgrading {nameof(Events.CohortApprovedByEmployer)} to publish with NServiceBus");
            return _endpointInstance.Publish(new CommitmentsV2.Messages.Events.CohortApprovedByEmployer()
            {
                AccountId = @event.AccountId,
                ProviderId = @event.ProviderId,
                CommitmentId = @event.CommitmentId
            });
        }
    }
}
