using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs
{
    public class EmployerAlertSummaryNotificationJob
    {
        private readonly IAlertSummaryService _alertSummaryService;
        private readonly ILogger<ImportStandardsJob> _logger;
        
        public EmployerAlertSummaryNotificationJob(IAlertSummaryService alertSummaryService, ILogger<ImportStandardsJob> logger)
        {
            _alertSummaryService = alertSummaryService;
            _logger = logger;
        }

        public async Task Import([TimerTrigger("0 7 * * 1-5", RunOnStartup = true)] TimerInfo timer)
        {
            _logger.LogInformation($"EmployerAlertSummaryNotificationJob - Started{(timer.IsPastDue ? " later than expected" : string.Empty)}");

            await _alertSummaryService.SendEmployerAlertSummaryNotifications();

            _logger.LogInformation("EmployerAlertSummaryNotificationJob - Finished");
        }
    }
}