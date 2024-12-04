using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.Services;

public class EmployerTransferRequestPendingEmailService(
    ITransferRequestDomainService transferRequestDomainService,
    IEncodingService encodingService,
    IMessageSession messageSession,
    ILogger<EmployerTransferRequestPendingEmailService> logger)
    : IEmployerTransferRequestPendingEmailService
{
    public async Task SendEmployerTransferRequestPendingNotifications()
    {
        logger.LogInformation($"Sending notifications for pending transfer requests");

        var employerAlertSummaryNotifications = await transferRequestDomainService.GetEmployerTransferRequestPendingNotifications();

        employerAlertSummaryNotifications.ForEach(pendingNotification => { SendEmail(pendingNotification, pendingNotification.SendingEmployerAccountId.Value, encodingService.Encode(pendingNotification.SendingEmployerAccountId.Value, EncodingType.AccountId)); });
    }

    private void SendEmail(EmployerTransferRequestPendingNotification notification, long accountId, string hashedAccountId)
    {
        var tokens = new Dictionary<string, string>
        {
            { "cohort_reference", notification.CohortReference },
            { "receiver_name", notification.ReceivingLegalEntityName },
            { "transfers_dashboard_url", $"accounts/{hashedAccountId}/transfers" }
        };

        messageSession.Send(new SendEmailToEmployerCommand(accountId, "SendingEmployerTransferRequestNotification", tokens));
    }
}