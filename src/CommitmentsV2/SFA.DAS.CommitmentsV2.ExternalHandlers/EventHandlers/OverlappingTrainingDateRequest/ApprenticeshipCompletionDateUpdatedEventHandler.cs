using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.ExternalHandlers.EventHandlers.OverlappingTrainingDateRequest;

public class ApprenticeshipCompletionDateUpdatedEventHandler(
    ILogger<ApprenticeshipCompletionDateUpdatedEvent> logger,
    IResolveOverlappingTrainingDateRequestService resolveOverlappingTrainingDateRequestService)
    : IHandleMessages<ApprenticeshipCompletionDateUpdatedEvent>
{
    public async Task Handle(ApprenticeshipCompletionDateUpdatedEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("Received Apprenticeship completion date updated event {ApprenticeshipId}", message.ApprenticeshipId);
        await resolveOverlappingTrainingDateRequestService.Resolve(message.ApprenticeshipId, null, OverlappingTrainingDateRequestResolutionType.CompletionDateEvent);
    }
}