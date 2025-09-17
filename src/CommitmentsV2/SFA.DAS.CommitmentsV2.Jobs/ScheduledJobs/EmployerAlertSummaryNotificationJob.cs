using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs;

public class EmployerAlertSummaryNotificationJob(IEmployerAlertSummaryEmailService alertSummaryService, ILogger<EmployerAlertSummaryNotificationJob> logger)
{
    public async Task Notify([TimerTrigger("0/15 * * * 1-5", RunOnStartup = false)] TimerInfo timer)
    {
        logger.LogInformation("EmployerAlertSummaryNotificationJob - Started {Time}",(timer?.IsPastDue ?? false ? " later than expected" : string.Empty));

        await alertSummaryService.SendEmployerAlertSummaryNotifications();

        logger.LogInformation("EmployerAlertSummaryNotificationJob - Finished");
    }
}