using System;
using System.Linq;
using System.Threading.Tasks;

using SFA.DAS.NLog.Logger;
using SFA.DAS.Notifications.Api.Client;
using System.Diagnostics;
using System.Collections.Generic;
using SFA.DAS.Notifications.Api.Types;

namespace SFA.DAS.Commitments.Notification.WebJob
{
    public class NotificationJob : INotificationJob
    {
        private readonly IEmployerEmailTemplatesService _emailTemplatesService;
        private readonly IProviderEmailTemplatesService _providerEmailTemplatesService;
        private readonly INotificationsApi _notificationsApi;
        private readonly ILog _logger;

        public NotificationJob(
            IEmployerEmailTemplatesService emailTemplatesService,
            IProviderEmailTemplatesService providerEmailTemplatesService,
            INotificationsApi notificationsApi,
            ILog logger
            )
        {
            _emailTemplatesService = emailTemplatesService;
            _providerEmailTemplatesService = providerEmailTemplatesService;
            _notificationsApi = notificationsApi;
            _logger = logger;
        }

        public async Task RunEmployerNotification()
        {
            var emails = await GetEmployerEmails();
            await SendEmails(emails);
        }

        public async Task RunProviderNotification()
        {
            var emails = await GetProviderEmails();
            await SendEmails(emails);
        }

        private async Task<IEnumerable<Email>> GetEmployerEmails()
        {
            var stopwatch = Stopwatch.StartNew();

            var emails = await _emailTemplatesService.GetEmails();

            _logger.Debug($"Took {stopwatch.ElapsedMilliseconds} milliseconds to determine emails to send", new Dictionary<string, object> { { "duration", stopwatch.ElapsedMilliseconds } });

            return emails;
        }

        private async Task<IEnumerable<Email>> GetProviderEmails()
        {
            var stopwatch = Stopwatch.StartNew();

            var emails = await _providerEmailTemplatesService.GetEmails();

            _logger.Debug($"Took {stopwatch.ElapsedMilliseconds} milliseconds to determine emails to send", new Dictionary<string, object> { { "duration", stopwatch.ElapsedMilliseconds } });

            return emails;
        }

        private async Task SendEmails(IEnumerable<Email> emails)
        {
            var emailsToSendCount = emails?.Count();

            if (emails == null || emailsToSendCount == 0)
            {
                _logger.Debug($"No emails to send");
                return;
            }

            var stopwatch = Stopwatch.StartNew();

            _logger.Debug($"About to send {emailsToSendCount} emails");

            var tasks = emails.Select(email => _notificationsApi.SendEmail(email));
            await Task.WhenAll(tasks);

            _logger.Debug($"Took {stopwatch.ElapsedMilliseconds} milliseconds to send {emailsToSendCount} emails", new Dictionary<string, object> { { "emailCount", emailsToSendCount }, { "duration", stopwatch.ElapsedMilliseconds } });
        }
    }
}