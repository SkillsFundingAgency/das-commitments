using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs
{
    public class ProviderAlertSummaryNotificationJob
    {
        private readonly IProviderAlertSummaryEmailService _alertSummaryService;
        private readonly ILogger<ProviderAlertSummaryNotificationJob> _logger;

        public ProviderAlertSummaryNotificationJob(IProviderAlertSummaryEmailService alertSummaryService, ILogger<ProviderAlertSummaryNotificationJob> logger)
        {
            _alertSummaryService = alertSummaryService;
            _logger = logger;
        }

        public async Task Import([TimerTrigger("0 7 * * 1-5", RunOnStartup = false)] TimerInfo timer)
        {
            _logger.LogInformation($"ProviderAlertSummaryNotificationJob - Started{(timer.IsPastDue ? " later than expected" : string.Empty)}");

            var notificationJobId = $"Notification.WJ.{DateTime.UtcNow.Ticks}.Provider";
            await _alertSummaryService.SendAlertSummaryEmails(notificationJobId);

            _logger.LogInformation("ProviderAlertSummaryNotificationJob - Finished");
        }
    }
}