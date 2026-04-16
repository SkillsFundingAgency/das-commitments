using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs;

public class ReplayApprenticeshipCreatedEventsJob(
    ILogger<ReplayApprenticeshipCreatedEventsJob> logger,
    IReplayInputFileStore replayInputFileStore,
    IReplayApprenticeshipCreatedEventsService replayApprenticeshipCreatedEventsService)
{
    public async Task Replay(
        [TimerTrigger("%SFA.DAS.CommitmentsV2:ReplayApprenticeshipCreatedEventsJobSchedule%", RunOnStartup = true)]
        TimerInfo timerInfo)
    {
        var pendingFiles = await replayInputFileStore.GetPendingFiles();
        if (pendingFiles.Count == 0)
        {
            logger.LogInformation("ReplayApprenticeshipCreatedEventsJob found no input files.");
            return;
        }

        logger.LogInformation("ReplayApprenticeshipCreatedEventsJob processing {Count} input file(s).", pendingFiles.Count);

        foreach (var pendingFile in pendingFiles)
        {
            await replayApprenticeshipCreatedEventsService.ReplayFromFile(pendingFile);
            await replayInputFileStore.ArchiveProcessedFile(pendingFile);
        }
    }
}
