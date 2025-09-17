using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs;

public class EmployerAlertSummaryNotificationJob(IEmployerAlertSummaryEmailService alertSummaryService, ILogger<EmployerAlertSummaryNotificationJob> logger)
{
    public async Task Notify([TimerTrigger("%SFA.DAS.CommitmentsV2:OLTDStopApprenticeshipAfter2WeeksJobSchedule%", RunOnStartup = false)] TimerInfo timer)
    {
        logger.LogInformation("EmployerAlertSummaryNotificationJob - Started {Time}",(timer?.IsPastDue ?? false ? " later than expected" : string.Empty));

        await alertSummaryService.SendEmployerAlertSummaryNotifications();

        logger.LogInformation("EmployerAlertSummaryNotificationJob - Finished");
    }
}