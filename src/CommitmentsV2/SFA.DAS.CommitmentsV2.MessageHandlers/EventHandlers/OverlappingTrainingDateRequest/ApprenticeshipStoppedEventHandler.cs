using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers.OverlappingTrainingDateRequest
{
    public class ApprenticeshipStoppedEventHandler : IHandleMessages<ApprenticeshipStoppedEvent>
    {
        private readonly IResolveOverlappingTrainingDateRequestService _resolveOverlappingTrainingDateRequestService;

        public ApprenticeshipStoppedEventHandler(IResolveOverlappingTrainingDateRequestService resolveOverlappingTrainingDateRequestService)
        {
            _resolveOverlappingTrainingDateRequestService = resolveOverlappingTrainingDateRequestService;
        }

        public async Task Handle(ApprenticeshipStoppedEvent message, IMessageHandlerContext context)
        {
            await _resolveOverlappingTrainingDateRequestService.Resolve(message.ApprenticeshipId, Types.OverlappingTrainingDateRequestResolutionType.ApprenticeshipStopped);
        }
  
    }
}
