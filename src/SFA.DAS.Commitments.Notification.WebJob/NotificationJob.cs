using System.Linq;
using System.Threading.Tasks;

using SFA.DAS.NLog.Logger;
using SFA.DAS.Notifications.Api.Client;
using System.Diagnostics;
using System.Collections.Generic;
using SFA.DAS.Commitments.Notification.WebJob.Configuration;
using SFA.DAS.Commitments.Notification.WebJob.EmailServices;
using SFA.DAS.Notifications.Api.Types;

namespace SFA.DAS.Commitments.Notification.WebJob
{
    public class NotificationJob : INotificationJob
    {
        private readonly IEmployerAlertSummaryEmailService _employerAlertsEmailService;
        private readonly IProviderAlertSummaryEmailService _providerAlertsEmailService;
        private readonly ISendingEmployerTransferRequestEmailService _sendingEmployerTransferRequestEmailService;
        private readonly INotificationsApi _notificationsApi;
        private readonly ILog _logger;

        private readonly CommitmentNotificationConfiguration _config;

        public NotificationJob(
            IEmployerAlertSummaryEmailService employerAlertsEmailService,
            IProviderAlertSummaryEmailService providerAlertsEmailService,
            ISendingEmployerTransferRequestEmailService sendingEmployerTransferRequestEmailService,
            INotificationsApi notificationsApi,
            ILog logger,
            CommitmentNotificationConfiguration config)
        {
            _employerAlertsEmailService = employerAlertsEmailService;
            _providerAlertsEmailService = providerAlertsEmailService;
            _sendingEmployerTransferRequestEmailService = sendingEmployerTransferRequestEmailService;
            _notificationsApi = notificationsApi;
            _logger = logger;
            _config = config;
        }

        public async Task RunEmployerAlertSummaryNotification(string jobId)
        {
            var emails = await GetEmployerEmails(jobId);
            await SendEmails(emails, jobId);
        }

        public async Task RunProviderAlertSummaryNotification(string jobId)
        {
            if (!_config.SendEmail)
            {
                _logger.Info($"Sending emails is turned off, JobId {jobId}");
                return;
            }
            await _providerAlertsEmailService.SendAlertSummaryEmails(jobId);
        }

        public async Task RunSendingEmployerTransferRequestNotification(string jobId)
        {
            var emails = await GetSendingEmployerTransferRequestEmails(jobId);
            await SendEmails(emails, jobId);
        }

        private async Task<IEnumerable<Email>> GetEmployerEmails(string jobId)
        {
            var stopwatch = Stopwatch.StartNew();

            var emails = await _employerAlertsEmailService.GetEmails();

            _logger.Debug($"Took {stopwatch.ElapsedMilliseconds} milliseconds to determine employer emails to send, JobId: {jobId}", 
                new Dictionary<string, object> { { "duration", stopwatch.ElapsedMilliseconds } });

            return emails;
        }

        private async Task<IEnumerable<Email>> GetSendingEmployerTransferRequestEmails(string jobId)
        {
            var stopwatch = Stopwatch.StartNew();

            var emails = await _sendingEmployerTransferRequestEmailService.GetEmails();

            _logger.Debug($"Took {stopwatch.ElapsedMilliseconds} milliseconds to determine sending employer transfer request emails to send, JobId: {jobId}",
                new Dictionary<string, object> { { "duration", stopwatch.ElapsedMilliseconds } });

            return emails;
        }

        private async Task SendEmails(IEnumerable<Email> emails, string jobId)
        {
            var emailsToSendCount = emails?.Count();

            if (emails == null || emailsToSendCount == 0)
            {
                _logger.Debug($"No emails to send, JobId: {jobId}");
                return;
            }

            if (!_config.SendEmail)
            {
                _logger.Info($"Sending emails is turned off, JobId {jobId}");
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            
            _logger.Debug($"About to send {emailsToSendCount} emails, JobId: {jobId}");
            
            var tasks = emails.Select(email => _notificationsApi.SendEmail(email));
            
            await Task.WhenAll(tasks);

            _logger.Debug($"Took {stopwatch.ElapsedMilliseconds} milliseconds to send {emailsToSendCount} emails, JobId; {jobId}", 
                new Dictionary<string, object>
                {
                    { "emailCount", emailsToSendCount },
                    { "duration", stopwatch.ElapsedMilliseconds },
                    { "JobId", jobId }
                });
        }
    }
}