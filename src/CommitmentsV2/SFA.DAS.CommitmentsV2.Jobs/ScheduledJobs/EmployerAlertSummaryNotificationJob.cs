using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs;

public class EmployerAlertSummaryNotificationJob(IEmployerAlertSummaryEmailService alertSummaryService, ILogger<EmployerAlertSummaryNotificationJob> logger)
{
    public async Task Notify([TimerTrigger("0 7 * * 1-5", RunOnStartup = false)] TimerInfo timer)
    {
        if (timer?.IsPastDue == true)
        {
            logger.LogInformation("EmployerAlertSummaryNotificationJob - Skipped past due run");
            return;
        }

        logger.LogInformation("EmployerAlertSummaryNotificationJob - Started");

        await alertSummaryService.SendEmployerAlertSummaryNotifications();

        logger.LogInformation("EmployerAlertSummaryNotificationJob - Finished");
    }
}