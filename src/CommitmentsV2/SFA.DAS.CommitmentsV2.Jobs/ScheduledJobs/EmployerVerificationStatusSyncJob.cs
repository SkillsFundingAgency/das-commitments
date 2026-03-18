using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs;

public class EmployerVerificationStatusSyncJob(
    IEmployerVerificationStatusSyncService syncService,
    ILogger<EmployerVerificationStatusSyncJob> logger)
{
    public async Task Sync([TimerTrigger("%SFA.DAS.CommitmentsV2:EmployerVerificationSyncSchedule%", RunOnStartup = false)] TimerInfo timer)
    {
        logger.LogInformation("Starting EmployerVerificationStatusSyncJob");
        await syncService.SyncPendingEmploymentChecksAsync();
        logger.LogInformation("EmployerVerificationStatusSyncJob completed");
    }
}
