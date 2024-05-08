using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class EmployerTransferRequestPendingEmailService : IEmployerTransferRequestPendingEmailService
    {
        private readonly ITransferRequestDomainService _transferRequestDomainService;
        private readonly IEncodingService _encodingService;
        private readonly IMessageSession _messageSession;
        private readonly ILogger<EmployerTransferRequestPendingEmailService> _logger;

        public EmployerTransferRequestPendingEmailService(ITransferRequestDomainService transferRequestDomainService, IEncodingService encodingService, IMessageSession messageSession, 
            ILogger<EmployerTransferRequestPendingEmailService> logger)
        {
            _transferRequestDomainService = transferRequestDomainService;
            _encodingService = encodingService;
            _messageSession = messageSession;
            _logger = logger;
        }

        public async Task SendEmployerTransferRequestPendingNotifications()
        {
            _logger.LogInformation($"Sending notifications for pending transfer requests");

            var employerAlertSummaryNotifications = await _transferRequestDomainService.GetEmployerTransferRequestPendingNotifications();

            employerAlertSummaryNotifications.ForEach(x => 
            {
                SendEmail(x, x.SendingEmployerAccountId.Value, _encodingService.Encode(x.SendingEmployerAccountId.Value, EncodingType.AccountId));
            });
        }

        private void SendEmail(EmployerTransferRequestPendingNotification notification, long accountId, string hashedAccountId)
        {
            var tokens =
                new Dictionary<string, string>
                {
                    {"cohort_reference", notification.CohortReference},
                    {"receiver_name", notification.ReceivingLegalEntityName},
                    {"transfers_dashboard_url", $"accounts/{hashedAccountId}/transfers"}
                };

            _messageSession.Send(new SendEmailToEmployerCommand(accountId, "SendingEmployerTransferRequestNotification", tokens));
        }
    }
}
