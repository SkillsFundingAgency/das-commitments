using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.Notifications.Messages.Commands;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers
{
    public class SendEmailToEmployerCommandHandler : IHandleMessages<SendEmailToEmployerCommand>
    {
        private const string Owner = "Owner";
        private const string Transactor = "Transactor";

        private readonly ILogger<SendEmailToEmployerCommandHandler> _logger;
        private readonly IAccountApiClient _accountApiClient;

        public SendEmailToEmployerCommandHandler(IAccountApiClient accountApiClient, ILogger<SendEmailToEmployerCommandHandler> logger)
        {
            _logger = logger;
            _accountApiClient = accountApiClient;
        }

        public async Task Handle(SendEmailToEmployerCommand message, IMessageHandlerContext context)
        {

            bool IsOwnerOrTransactor(string role)
            {
                return role.Equals(Owner, StringComparison.InvariantCultureIgnoreCase) ||
                       role.Equals(Transactor, StringComparison.InvariantCultureIgnoreCase);
            }

            try
            {
                _logger.LogInformation($"Getting AccountUsers for {message.AccountId}");

                var emails = new List<string>();
                var users = await _accountApiClient.GetAccountUsers(message.AccountId);

                if (string.IsNullOrWhiteSpace(message.EmailAddress))
                {
                    _logger.LogInformation("Sending emails to all AccountUsers who can recieve emails");
                    emails.AddRange(users.Where(x =>
                            x.CanReceiveNotifications && !string.IsNullOrWhiteSpace(x.Email) &&
                            IsOwnerOrTransactor(x.Role))
                        .Select(x => x.Email));
                }
                else if (users.Any(x => message.EmailAddress.Equals(x.Email, StringComparison.InvariantCultureIgnoreCase) &&
                    x.CanReceiveNotifications))
                {
                    _logger.LogInformation("Sending email to the explicit user in message");
                    emails.Add(message.EmailAddress);
                }

                if (emails.Any())
                {
                    _logger.LogInformation($"Calling SendEmailCommand for {emails.Count()} emails");
                    await Task.WhenAll(emails.Select(email =>
                        context.Send(new SendEmailCommand(message.Template,  email, message.Tokens))));
                }
                else
                {
                    _logger.LogWarning($"No Email Addresses found to send Template {message.Template} for AccountId {message.AccountId}");
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Error processing {nameof(SendEmailToEmployerCommand)}", e);
                throw;
            }
        }
    }
}