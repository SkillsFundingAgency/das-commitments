using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.ExternalHandlers.EventHandlers.OverlappingTrainingDateRequest
{
    public class ApprenticeshipCompletionDateUpdatedEventHandler : IHandleMessages<ApprenticeshipCompletionDateUpdatedEvent>
    {
        private readonly ILogger<ApprenticeshipCompletionDateUpdatedEvent> _logger;
        private readonly IResolveOverlappingTrainingDateRequestService _resolveOverlappingTrainingDateRequestService;
        public ApprenticeshipCompletionDateUpdatedEventHandler(ILogger<ApprenticeshipCompletionDateUpdatedEvent> logger, IResolveOverlappingTrainingDateRequestService resolveOverlappingTrainingDateRequestService)
        {

            _logger = logger;
            _resolveOverlappingTrainingDateRequestService = resolveOverlappingTrainingDateRequestService;
        }

        public async Task Handle(ApprenticeshipCompletionDateUpdatedEvent message, IMessageHandlerContext context)
        {
            _logger.LogInformation($"Recieved Apprenticeship completion date updated event {message.ApprenticeshipId}");
            await _resolveOverlappingTrainingDateRequestService.Resolve(message.ApprenticeshipId, null, OverlappingTrainingDateRequestResolutionType.CompletionDateEvent);
        }
    }
}
