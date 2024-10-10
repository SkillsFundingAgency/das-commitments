using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs;

public class ProviderAlertSummaryNotificationJob(IProviderAlertSummaryEmailService alertSummaryService, ILogger<ProviderAlertSummaryNotificationJob> logger)
{
    public async Task Import([TimerTrigger("0 7 * * 1-5", RunOnStartup = false)] TimerInfo timer)
    {
        logger.LogInformation("ProviderAlertSummaryNotificationJob - Started {At}", timer.IsPastDue ? " later than expected" : string.Empty);

        var notificationJobId = $"Notification.WJ.{DateTime.UtcNow.Ticks}.Provider";
        await alertSummaryService.SendAlertSummaryEmails(notificationJobId);

        logger.LogInformation("ProviderAlertSummaryNotificationJob - Finished");
    }
}