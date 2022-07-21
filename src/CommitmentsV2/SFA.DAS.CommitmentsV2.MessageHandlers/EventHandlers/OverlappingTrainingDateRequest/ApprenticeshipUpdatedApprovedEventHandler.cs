using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers.OverlappingTrainingDateRequest
{
    public class ApprenticeshipUpdatedApprovedEventHandler : IHandleMessages<ApprenticeshipUpdatedApprovedEvent>
    {
        private readonly IResolveOverlappingTrainingDateRequestService _resolveOverlappingTrainingDateRequestService;

        public ApprenticeshipUpdatedApprovedEventHandler(IResolveOverlappingTrainingDateRequestService resolveOverlappingTrainingDateRequestService)
        {
            _resolveOverlappingTrainingDateRequestService = resolveOverlappingTrainingDateRequestService;
        }

        public async Task Handle(ApprenticeshipUpdatedApprovedEvent message, IMessageHandlerContext context)
        {
            await _resolveOverlappingTrainingDateRequestService.Resolve(message.ApprenticeshipId, Types.OverlappingTrainingDateRequestResolutionType.ApprenticeshipUpdate);
        }
    }
}
