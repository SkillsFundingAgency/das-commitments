using SFA.DAS.CommitmentsV2.Application.Commands.OverlappingTrainingDateRequestAutomaticStopAfter2Weeks;

namespace SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs
{
    public class OverlappingTrainingDateRequestAutomaticStopAfter2WeeksJob
    {
        private readonly ILogger<OverlappingTrainingDateRequestAutomaticStopAfter2WeeksJob> _logger;
        private readonly IMediator _mediator;

        public OverlappingTrainingDateRequestAutomaticStopAfter2WeeksJob(ILogger<OverlappingTrainingDateRequestAutomaticStopAfter2WeeksJob> logger,
            IMediator mediator)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task StopApprenticeships([TimerTrigger("%SFA.DAS.CommitmentsV2:OLTDStopApprenticeshipAfter2WeeksJobSchedule%", RunOnStartup = true)] TimerInfo timer)
        {
            _logger.LogInformation("Starting OverlappingTrainingDateRequestAutomaticStopAfter2WeeksJob");

            await _mediator.Send(new OverlappingTrainingDateRequestAutomaticStopAfter2WeeksCommand());

            _logger.LogInformation("OverlappingTrainingDateRequestAutomaticStopAfter2WeeksJob - Finished");
        }
    }
}
