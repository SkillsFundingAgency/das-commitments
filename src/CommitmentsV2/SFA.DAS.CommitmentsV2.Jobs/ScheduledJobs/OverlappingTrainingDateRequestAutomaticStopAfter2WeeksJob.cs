using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.OverlappingTrainingDateRequestNotificationToServiceDesk;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs
{
    public class OverlappingTrainingDateRequestAutomaticStopAfter2WeeksJob
    {
        private readonly ILogger<OverlappingTrainingDateRequestAutomaticStopAfter2WeeksJob> _logger;
        private readonly IAutomaticStopOverlappingTrainingDateRequestsService _automaticStopOverlappingTrainingDateRequestsService;
        private readonly IMediator _mediator;

        public OverlappingTrainingDateRequestAutomaticStopAfter2WeeksJob(
            IMediator mediator,
            IAutomaticStopOverlappingTrainingDateRequestsService automaticStopOverlappingTrainingDateRequestsService,
            ILogger<OverlappingTrainingDateRequestAutomaticStopAfter2WeeksJob> logger)
        {
            _mediator = mediator;
            _automaticStopOverlappingTrainingDateRequestsService = automaticStopOverlappingTrainingDateRequestsService;
            _logger = logger;
        }

        public async Task StopApprenticeships([TimerTrigger("%SFA.DAS.CommitmentsV2:OLTDStopApprenticeshipAfter2WeeksJobSchedule%", RunOnStartup = false)] TimerInfo timer)
        {
            _logger.LogInformation("Starting OverlappingTrainingDateRequestAutomaticStopAfter2WeeksJob");

            await _automaticStopOverlappingTrainingDateRequestsService.AutomaticallyStopOverlappingTrainingDateRequests();

            await _mediator.Send(new OverlappingTrainingDateRequestNotificationToServiceDeskCommand());

            _logger.LogInformation("OverlappingTrainingDateRequestAutomaticStopAfter2WeeksJob - Finished");
        }
    }
}
