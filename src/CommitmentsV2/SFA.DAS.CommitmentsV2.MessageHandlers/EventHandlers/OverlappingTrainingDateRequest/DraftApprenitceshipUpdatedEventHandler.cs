using NServiceBus;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers.OverlappingTrainingDateRequest
{
    public class DraftApprenitceshipUpdatedEventHandler : IHandleMessages<DraftApprenticeshipUpdatedEvent>
    {
        private readonly IResolveOverlappingTrainingDateRequestService _resolveOverlappingTrainingDateRequestService;

        public DraftApprenitceshipUpdatedEventHandler(IResolveOverlappingTrainingDateRequestService resolveOverlappingTrainingDateRequestService)
        {
            _resolveOverlappingTrainingDateRequestService = resolveOverlappingTrainingDateRequestService;
        }

        public async Task Handle(DraftApprenticeshipUpdatedEvent message, IMessageHandlerContext context)
        {
            await _resolveOverlappingTrainingDateRequestService.ResolveByDraftApprenticeshp(message.DraftApprenticeshipId, Types.OverlappingTrainingDateRequestResolutionType.ApprenticeshipStopped);
        }
    }
}
