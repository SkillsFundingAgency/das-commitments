using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs;

public class DataLockUpdaterJobs(ILogger<DataLockUpdaterJobs> logger, IDataLockUpdaterService dataLockUpdaterService)
{
    public async Task Update([TimerTrigger("*/30 * * * * *", RunOnStartup = false)] TimerInfo timer)
    {
        logger.LogInformation("DataLockUpdaterJobs - Started {Time}",timer?.IsPastDue ?? false ? " later than expected" : string.Empty);

        await dataLockUpdaterService.RunUpdate();

        logger.LogInformation("DataLockUpdaterJobs - Finished");
    }
}