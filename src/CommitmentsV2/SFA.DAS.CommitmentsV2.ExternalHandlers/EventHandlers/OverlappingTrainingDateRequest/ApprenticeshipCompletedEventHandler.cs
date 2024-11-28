using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.ExternalHandlers.EventHandlers.OverlappingTrainingDateRequest;

public class ApprenticeshipCompletedEventHandler(
    ILogger<ApprenticeshipCompletedEventHandler> logger,
    IResolveOverlappingTrainingDateRequestService resolveOverlappingTrainingDateRequestService)
    : IHandleMessages<ApprenticeshipCompletedEvent>
{
    public async Task Handle(ApprenticeshipCompletedEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("Received Apprenticeship completion event {ApprenticeshipId}", message.ApprenticeshipId);
        await resolveOverlappingTrainingDateRequestService.Resolve(message.ApprenticeshipId, null, OverlappingTrainingDateRequestResolutionType.CompletionDateEvent);
    }
}