using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers.OverlappingTrainingDateRequest;

public class DraftApprenticeshipDeletedEventHandler(IResolveOverlappingTrainingDateRequestService resolveOverlappingTrainingDateRequestService)
    : IHandleMessages<DraftApprenticeshipDeletedEvent>
{
    public async Task Handle(DraftApprenticeshipDeletedEvent message, IMessageHandlerContext context)
    {
        await resolveOverlappingTrainingDateRequestService.DraftApprenticeshpDeleted(message.DraftApprenticeshipId, Types.OverlappingTrainingDateRequestResolutionType.DraftApprenticeshipDeleted);
    }
}