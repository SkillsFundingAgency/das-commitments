using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types;
using SFA.DAS.Notifications.Messages.Commands;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers;

public class SendEmailToProviderCommandHandler(IApprovalsOuterApiClient approvalsOuterApiClient, ILogger<SendEmailToProviderCommandHandler> logger)
    : IHandleMessages<SendEmailToProviderCommand>
{
    public async Task Handle(SendEmailToProviderCommand message, IMessageHandlerContext context)
    {
        try
        {
            var providerUsersResponse = await approvalsOuterApiClient.Get<ProvidersUsersResponse>(new GetProviderUsersRequest(message.ProviderId));

            if (providerUsersResponse == null)
            {
                logger.LogWarning("No users found for ProviderId {ProviderId}", message.ProviderId);
                return;
            }

            var explicitEmailAddresses = string.IsNullOrWhiteSpace(message.EmailAddress)
                ? []
                : new List<string> { message.EmailAddress };

            List<string> recipients;

            if (explicitEmailAddresses.Count != 0)
            {
                logger.LogInformation("Explicit recipients requested for email");

                recipients = explicitEmailAddresses;
            }
            else
            {
                recipients = providerUsersResponse.Users.Any(u => !u.IsSuperUser) ?
                    providerUsersResponse.Users.Where(x => !x.IsSuperUser).Select(x => x.EmailAddress).ToList():
                    providerUsersResponse.Users.Select(x => x.EmailAddress).ToList();
            }

            var optedOutList = providerUsersResponse.Users.Where(x => !x.ReceiveNotifications).Select(x => x.EmailAddress).ToList();

            var finalRecipients = recipients
                .Where(x => !optedOutList.Exists(y => x.Equals(y, StringComparison.CurrentCultureIgnoreCase)))
                .ToList();

            if (finalRecipients.Count != 0)
            {
                logger.LogInformation("Calling SendEmailCommand for {Count} emails", finalRecipients.Count);
                var tasks = finalRecipients.Select(email => context.Send(new SendEmailCommand(message.Template, email, message.Tokens)));
                await Task.WhenAll(tasks);

                logger.LogInformation("Sent email to {Count} recipients for ukprn: {ProviderId}", finalRecipients.Count, message.ProviderId);
            }
            else
            {
                logger.LogWarning("No Email Addresses found to send Template {Template} for ProviderId {ProviderId}", message.Template, message.ProviderId);
            }

        }
        catch (Exception e)
        {
            logger.LogError(e, "Error processing {TypeName}", nameof(SendEmailToProviderCommand));
            throw;
        }
    }
}