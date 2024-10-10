using SFA.DAS.CommitmentsV2.Application.Commands.OverlappingTrainingDateRequestAutomaticStopAfter2Weeks;

namespace SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs
{
    public class OverlappingTrainingDateRequestAutomaticStopAfter2WeeksJob(
        ILogger<OverlappingTrainingDateRequestAutomaticStopAfter2WeeksJob> logger,
        IMediator mediator)
    {
        public async Task StopApprenticeships([TimerTrigger("%SFA.DAS.CommitmentsV2:OLTDStopApprenticeshipAfter2WeeksJobSchedule%", RunOnStartup = false)] TimerInfo timer)
        {
            logger.LogInformation("Starting OverlappingTrainingDateRequestAutomaticStopAfter2WeeksJob");

            await mediator.Send(new OverlappingTrainingDateRequestAutomaticStopAfter2WeeksCommand());

            logger.LogInformation("OverlappingTrainingDateRequestAutomaticStopAfter2WeeksJob - Finished");
        }
    }
}
