using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs
{
    public class EmployerAlertSummaryNotificationJob
    {
        private readonly IEmployerAlertSummaryEmailService _employerAlertSummaryEmailService;
        private readonly ILogger<EmployerAlertSummaryNotificationJob> _logger;
        
        public EmployerAlertSummaryNotificationJob(IEmployerAlertSummaryEmailService alertSummaryService, ILogger<EmployerAlertSummaryNotificationJob> logger)
        {
            _employerAlertSummaryEmailService = alertSummaryService;
            _logger = logger;
        }

        public async Task Notify([TimerTrigger("0 7 * * 1-5", RunOnStartup = true)] TimerInfo timer)
        {
            _logger.LogInformation($"EmployerAlertSummaryNotificationJob - Started{(timer?.IsPastDue ?? false ? " later than expected" : string.Empty)}");

            await _employerAlertSummaryEmailService.SendEmployerAlertSummaryNotifications();

            _logger.LogInformation("EmployerAlertSummaryNotificationJob - Finished");
        }
    }
}