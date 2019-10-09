using System;
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
        private readonly ILogger<SendEmailToEmployerCommandHandler> _logger;
        private readonly IAccountApiClient _accountApiClient;

        public SendEmailToEmployerCommandHandler(ILogger<SendEmailToEmployerCommandHandler> logger, IAccountApiClient accountApiClient)
        {
            _logger = logger;
            _accountApiClient = accountApiClient;
        }

        public async Task Handle(SendEmailToEmployerCommand message, IMessageHandlerContext context)
        {
            bool IsOwnerOrTransactor(string role)
            {
                return role.Equals("Owner", StringComparison.InvariantCultureIgnoreCase) ||
                       role.Equals("Transactor", StringComparison.InvariantCultureIgnoreCase);
            }

            if (!string.IsNullOrWhiteSpace(message.EmailAddress))
            {
                var users = await _accountApiClient.GetAccountUsers(message.AccountId);
                var list = users.Where(x =>
                    x.CanReceiveNotifications && !string.IsNullOrWhiteSpace(x.Email) && IsOwnerOrTransactor(x.Role)).ToList();

                if (list.Any())
                {
                    await Task.WhenAll(list.Select(x => context.Publish(new {x.Email, message.Template, message.Tokens})));
                }
            }
        }
    }
}