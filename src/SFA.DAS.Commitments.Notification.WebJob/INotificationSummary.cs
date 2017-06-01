using System.Linq;
using System.Threading.Tasks;

using SFA.DAS.NLog.Logger;
using SFA.DAS.Notifications.Api.Client;

namespace SFA.DAS.Commitments.Notification.WebJob
{
    public class NotificationJob : INotificationJob
    {
        private readonly IEmailTemplatesService _emailTemplatesService;
        private readonly INotificationsApi _notificationsApi;
        private readonly ILog _logger;

        public NotificationJob(
            IEmailTemplatesService emailTemplatesService,
            INotificationsApi notificationsApi,
            ILog logger
            )
        {
            _emailTemplatesService = emailTemplatesService;
            _notificationsApi = notificationsApi;
            _logger = logger;
        }

        public async Task Run()
        {
            var emails = await _emailTemplatesService.GetEmails();

            _logger.Trace($"Will start sending {emails.Count()} emails");
            foreach (var email in emails)
            {
                await _notificationsApi.SendEmail(email);
            }
        }
    }
}