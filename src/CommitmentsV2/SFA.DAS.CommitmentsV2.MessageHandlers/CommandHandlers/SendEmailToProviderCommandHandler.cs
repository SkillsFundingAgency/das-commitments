using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types;
using SFA.DAS.Notifications.Messages.Commands;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers;

public class SendEmailToProviderCommandHandler(
    IApprovalsOuterApiClient approvalsOuterApiClient, 
    ILogger<SendEmailToProviderCommandHandler> logger)
    : IHandleMessages<SendEmailToProviderCommand>
{
    public async Task Handle(SendEmailToProviderCommand message, IMessageHandlerContext context)
    {
        try
        {
            var providerUsersResponse = await approvalsOuterApiClient.Get<ProvidersUsersResponse>(new GetProviderUsersRequest(message.ProviderId));

            if (providerUsersResponse == null)
            {
                logger.LogWarning("{TypeName}. No users found for ProviderId {ProviderId}", nameof(SendEmailToProviderCommand), message.ProviderId);
                return;
            }

            var explicitEmailAddresses = string.IsNullOrWhiteSpace(message.EmailAddress)
                ? []
                : new List<string> { message.EmailAddress };

            List<string> recipients;

            if (explicitEmailAddresses.Any())
            {
                logger.LogInformation("{TypeName}. Explicit recipients requested for email", nameof(SendEmailToProviderCommand));

                recipients = explicitEmailAddresses;
            }
            else
            {
                recipients = providerUsersResponse.Users.Any(u => !u.IsSuperUser) ? providerUsersResponse.Users.Where(x => !x.IsSuperUser).Select(x => x.EmailAddress).ToList() : providerUsersResponse.Users.Select(x => x.EmailAddress).ToList();
            }

            var optedOutList = providerUsersResponse.Users.Where(x => !x.ReceiveNotifications).Select(x => x.EmailAddress).ToList();

            var finalRecipients = recipients
                .Where(x => !optedOutList.Exists(y => x.Equals(y, StringComparison.CurrentCultureIgnoreCase)))
                .ToList();

            if (finalRecipients.Any())
            {
                logger.LogInformation("{TypeName}. Total SendEmailCommand to provider calls: {Count}.", nameof(SendEmailToProviderCommand), finalRecipients.Count);

                var tasks = finalRecipients.Select(email => context.Send(new SendEmailCommand(message.Template, email, message.Tokens)));

                await Task.WhenAll(tasks);

                logger.LogInformation("{TypeName}. Sent email to {Count} provider recipients for ukprn: {ProviderId}", nameof(SendEmailToProviderCommand), finalRecipients.Count, message.ProviderId);
            }
            else
            {
                logger.LogWarning("{TypeName}. No Email Addresses found to send Template {Template} for ProviderId {ProviderId}", nameof(SendEmailToProviderCommand), message.Template, message.ProviderId);
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error processing {TypeName}", nameof(SendEmailToProviderCommand));
            throw;
        }
    }
}