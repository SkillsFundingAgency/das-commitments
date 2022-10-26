using NServiceBus;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers.OverlappingTrainingDateRequest
{
    public class DraftApprenticeshipDeletedEventHandler : IHandleMessages<DraftApprenticeshipDeletedEvent>
    {
        private readonly IResolveOverlappingTrainingDateRequestService _resolveOverlappingTrainingDateRequestService;

        public DraftApprenticeshipDeletedEventHandler(IResolveOverlappingTrainingDateRequestService resolveOverlappingTrainingDateRequestService)
        {
            _resolveOverlappingTrainingDateRequestService = resolveOverlappingTrainingDateRequestService;
        }

        public async Task Handle(DraftApprenticeshipDeletedEvent message, IMessageHandlerContext context)
        {
            await _resolveOverlappingTrainingDateRequestService.DraftApprenticeshpDeleted(message.DraftApprenticeshipId, Types.OverlappingTrainingDateRequestResolutionType.DraftApprenticeshipDeleted);
        }
    }
}
