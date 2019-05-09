using NServiceBus;
using SFA.DAS.Commitments.Application.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Application.Services
{
    public class EventUpgradeHandler :
        IEventUpgradeHandler<Events.CohortApprovalRequestedByProvider>
    {
        private readonly IEndpointInstance _endpointInstance;

        public EventUpgradeHandler(IEndpointInstance endpointInstance)
        {
            _endpointInstance = endpointInstance;
        }

        public Task Execute(Events.CohortApprovalRequestedByProvider @event)
        {
            return _endpointInstance.Publish(new CommitmentsV2.Messages.Events.CohortApprovalRequestedByProvider()
            {
                AccountId = @event.AccountId,
                ProviderId = @event.ProviderId,
                CommitmentId = @event.CommitmentId
            });
        }
    }
}
