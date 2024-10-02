using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.Notifications.Messages.Commands;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers;

public class SendEmailToEmployerCommandHandler(
    IAccountApiClient accountApiClient, 
    ILogger<SendEmailToEmployerCommandHandler> logger)
    : IHandleMessages<SendEmailToEmployerCommand>
{
    private const string Owner = "Owner";
    private const string Transactor = "Transactor";

    public async Task Handle(SendEmailToEmployerCommand message, IMessageHandlerContext context)
    {
        try
        {
            logger.LogInformation("Getting AccountUsers for {AccountId}", message.AccountId);

            var emails = new List<(string Email, string Name)>();
            var users = await accountApiClient.GetAccountUsers(message.AccountId);

            if (string.IsNullOrWhiteSpace(message.EmailAddress))
            {
                logger.LogInformation("Sending emails to all AccountUsers who can receive emails");

                emails.AddRange(users.Where(x =>
                        x.CanReceiveNotifications && !string.IsNullOrWhiteSpace(x.Email) &&
                        IsOwnerOrTransactor(x.Role))
                    .Select(x => (x.Email, x.Name)));
            }
            else
            {
                var user = users.FirstOrDefault(x =>
                    x.CanReceiveNotifications && message.EmailAddress.Equals(x.Email, StringComparison.InvariantCultureIgnoreCase));

                if (user != null)
                {
                    logger.LogInformation("Sending email to the explicit user in message");
                    emails.Add((Email: message.EmailAddress, user.Name));
                }
            }

            if (emails.Count != 0)
            {
                logger.LogInformation("Calling SendEmailCommand for {Count} emails", emails.Count);

                var emailTasks = emails.Select(email =>
                {
                    var tokens = new Dictionary<string, string>(message.Tokens);
                    if (!string.IsNullOrEmpty(message.NameToken))
                    {
                        tokens.Add(message.NameToken, email.Name);
                    }

                    return context.Send(new SendEmailCommand(message.Template, email.Email, tokens));
                });

                await Task.WhenAll(emailTasks);
            }
            else
            {
                logger.LogWarning("No Email Addresses found to send Template {Template} for AccountId {AccountId}", message.Template, message.AccountId);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error processing {CommandName)}", nameof(SendEmailToEmployerCommand));
            throw;
        }
    }
    
    private static bool IsOwnerOrTransactor(string role)
    {
        return role.Equals(Owner, StringComparison.InvariantCultureIgnoreCase) ||
               role.Equals(Transactor, StringComparison.InvariantCultureIgnoreCase);
    }
}