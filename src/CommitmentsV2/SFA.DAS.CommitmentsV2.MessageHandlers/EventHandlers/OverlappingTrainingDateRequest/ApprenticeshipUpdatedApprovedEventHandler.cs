using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers.OverlappingTrainingDateRequest;

public class ApprenticeshipUpdatedApprovedEventHandler(IResolveOverlappingTrainingDateRequestService resolveOverlappingTrainingDateRequestService)
    : IHandleMessages<ApprenticeshipUpdatedApprovedEvent>
{
    public async Task Handle(ApprenticeshipUpdatedApprovedEvent message, IMessageHandlerContext context)
    {
        await resolveOverlappingTrainingDateRequestService.Resolve(message.ApprenticeshipId,null, Types.OverlappingTrainingDateRequestResolutionType.ApprenticeshipUpdate);
    }
}