using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs;

public class EmployerTransferRequestPendingNotificationJob(IEmployerTransferRequestPendingEmailService employerTransferRequestPendingEmailService, ILogger<EmployerTransferRequestPendingNotificationJob> logger)
{
    public async Task Notify([TimerTrigger("0 7 * * 1-5", RunOnStartup = false)] TimerInfo timer)
    {
        logger.LogInformation("EmployerTransferRequestPendingNotificationJob - Started {Time}",timer?.IsPastDue ?? false ? " later than expected" : string.Empty);

        await employerTransferRequestPendingEmailService.SendEmployerTransferRequestPendingNotifications();

        logger.LogInformation("EmployerTransferRequestPendingNotificationJob - Finished");
    }
}