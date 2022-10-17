using System.Threading.Tasks;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers.OverlappingTrainingDateRequest
{
    public class ApprenticeshipStopDateChangedEventHandler
    {
        private readonly IResolveOverlappingTrainingDateRequestService _resolveOverlappingTrainingDateRequestService;

        public ApprenticeshipStopDateChangedEventHandler(IResolveOverlappingTrainingDateRequestService resolveOverlappingTrainingDateRequestService)
        {
            _resolveOverlappingTrainingDateRequestService = resolveOverlappingTrainingDateRequestService;
        }

        public async Task Handle(ApprenticeshipStopDateChangedEvent message, IMessageHandlerContext context)
        {
            await _resolveOverlappingTrainingDateRequestService.Resolve(message.ApprenticeshipId, null, Types.OverlappingTrainingDateRequestResolutionType.StopDateUpdate);
        }
    }
}
