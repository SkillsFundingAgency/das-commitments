using SFA.DAS.CommitmentsV2.Application.Commands.ExpireInactiveCohortsWithEmployerAfter2Weeks;

namespace SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs;
public class ExpireInactiveCohortsWithEmployerAfter2Weeks(
      ILogger<ExpireInactiveCohortsWithEmployerAfter2Weeks> logger,
      IMediator mediator)
{
    public async Task StopApprenticeships([TimerTrigger("%SFA.DAS.CommitmentsV2:ExpireInactiveCohortsWithEmployerAfter2WeeksSchedule%", RunOnStartup = false)] TimerInfo timer)
    {
        logger.LogInformation("Starting ExpireInactiveCohortsWithEmployerAfter2Weeks");

        await mediator.Send(new ExpireInactiveCohortsWithEmployerAfter2WeeksCommand());

        logger.LogInformation("ExpireInactiveCohortsWithEmployerAfter2Weeks - Finished");
    }
}
