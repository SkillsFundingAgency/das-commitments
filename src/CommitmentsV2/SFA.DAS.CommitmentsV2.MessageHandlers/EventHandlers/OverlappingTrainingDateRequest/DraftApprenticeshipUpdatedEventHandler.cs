using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers.OverlappingTrainingDateRequest;

public class DraftApprenticeshipUpdatedEventHandler(
    IResolveOverlappingTrainingDateRequestService resolveOverlappingTrainingDateRequestService)
    : IHandleMessages<DraftApprenticeshipUpdatedEvent>
{
    public async Task Handle(DraftApprenticeshipUpdatedEvent message, IMessageHandlerContext context)
    {
        await resolveOverlappingTrainingDateRequestService.Resolve(null, message.DraftApprenticeshipId, Types.OverlappingTrainingDateRequestResolutionType.DraftApprenticeshipUpdated);
    }
}