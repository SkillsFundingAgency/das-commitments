using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.EAS.Account.Api.Client;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers
{
    public class SendEmailToEmployerCommandHandler : IHandleMessages<SendEmailToEmployerCommand>
    {
        private const string Owner = "Owner";
        private const string Transactor = "Transactor";
        private const string ReplyTo = "noreply@sfa.gov.uk";

        private readonly ILogger<SendEmailToEmployerCommandHandler> _logger;
        private readonly IAccountApiClient _accountApiClient;

        public SendEmailToEmployerCommandHandler(IAccountApiClient accountApiClient, ILogger<SendEmailToEmployerCommandHandler> logger)
        {
            _logger = logger;
            _accountApiClient = accountApiClient;
        }

        public async Task Handle(SendEmailToEmployerCommand message, IMessageHandlerContext context)
        {
            try
            {
                List<string> emails;

                bool IsOwnerOrTransactor(string role)
                {
                    return role.Equals(Owner, StringComparison.InvariantCultureIgnoreCase) ||
                           role.Equals(Transactor, StringComparison.InvariantCultureIgnoreCase);
                }

                if (string.IsNullOrWhiteSpace(message.EmailAddress))
                {
                    var users = await _accountApiClient.GetAccountUsers(message.AccountId);
                    emails = users.Where(x =>
                            x.CanReceiveNotifications && !string.IsNullOrWhiteSpace(x.Email) &&
                            IsOwnerOrTransactor(x.Role))
                        .Select(x => x.Email).ToList();
                }
                else
                {
                    emails = new List<string> {message.EmailAddress};
                }

                if (emails.Any())
                {
                    await Task.WhenAll(emails.Select(email =>
                        context.Publish(new SendEmailCommand(message.Template, email, ReplyTo, message.Tokens))));
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Error processing SendEmailToEmployerCommand", e);
                throw;
            }
        }
    }
}