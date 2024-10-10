using SFA.DAS.CommitmentsV2.Application.Commands.OverlappingTrainingDateRequestNotificationToEmployer;

namespace SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs;

public class OverlappingTrainingDateRequestNotificationToEmployerJob(IMediator mediator, ILogger<OverlappingTrainingDateRequestNotificationToEmployerJob> logger)
{
    public async Task Notify([TimerTrigger("%SFA.DAS.CommitmentsV2:OLTDNotificationToEmployerJobSchedule%", RunOnStartup = false)] TimerInfo timer)
    {
        logger.LogInformation("Starting OverlappingTrainingDateRequestNotificationToEmployerJob");
        await mediator.Send(new OverlappingTrainingDateRequestNotificationToEmployerCommand());
    }
}