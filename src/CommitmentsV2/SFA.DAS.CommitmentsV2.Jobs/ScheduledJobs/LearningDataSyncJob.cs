using SFA.DAS.CommitmentsV2.Application.Commands.LearningDataSync;

namespace SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs;

public class LearningDataSyncJob(ILogger<LearningDataSyncJob> logger, IMediator mediator)
{
    public async Task SyncData(
        [TimerTrigger("%SFA.DAS.CommitmentsV2:LearningDataSyncJobSchedule%", RunOnStartup = false)]
        TimerInfo timer)
    {
        logger.LogInformation("LearningDataSyncJob triggered");

        await mediator.Send(new LearningDataSyncCommand());

        logger.LogInformation("LearningDataSyncJob ended");
    }
}

