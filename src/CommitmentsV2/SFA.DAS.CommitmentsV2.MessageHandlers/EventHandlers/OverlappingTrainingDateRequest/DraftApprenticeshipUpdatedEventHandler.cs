using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers.OverlappingTrainingDateRequest
{
    public class DraftApprenticeshipUpdatedEventHandler : IHandleMessages<DraftApprenticeshipUpdatedEvent>
    {
        private readonly IResolveOverlappingTrainingDateRequestService _resolveOverlappingTrainingDateRequestService;

        public DraftApprenticeshipUpdatedEventHandler(IResolveOverlappingTrainingDateRequestService resolveOverlappingTrainingDateRequestService)
        {
            _resolveOverlappingTrainingDateRequestService = resolveOverlappingTrainingDateRequestService;
        }

        public async Task Handle(DraftApprenticeshipUpdatedEvent message, IMessageHandlerContext context)
        {
            await _resolveOverlappingTrainingDateRequestService.Resolve(null, message.DraftApprenticeshipId, Types.OverlappingTrainingDateRequestResolutionType.DraftApprenticeshipUpdated);
        }
    }
}
