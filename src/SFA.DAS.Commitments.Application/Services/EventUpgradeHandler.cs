using NServiceBus;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Events;
using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Application.Services
{
    public class EventUpgradeHandler :
        IEventUpgradeHandler<CohortApprovalRequestedByProvider>
    {
        private readonly IEndpointInstance _endpointInstance;

        public EventUpgradeHandler(IEndpointInstance endpointInstance)
        {
            _endpointInstance = endpointInstance;
        }

        public Task Execute(CohortApprovalRequestedByProvider @event)
        {
            return _endpointInstance.Publish(@event);
        }
    }
}
