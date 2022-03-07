using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.Notifications.Messages.Commands;
using SFA.DAS.PAS.Account.Api.ClientV2;
using SFA.DAS.PAS.Account.Api.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers
{
    public class SendEmailToProviderCommandHandler : IHandleMessages<SendEmailToProviderCommand>
    {
        private readonly IPasAccountApiClient _pasAccountApi;
        private readonly ILogger<SendEmailToProviderCommandHandler> _logger;

        public SendEmailToProviderCommandHandler(IPasAccountApiClient pasAccountApi, ILogger<SendEmailToProviderCommandHandler> logger)
        {
            _pasAccountApi = pasAccountApi;
            _logger = logger;
        }

        public async Task Handle(SendEmailToProviderCommand message, IMessageHandlerContext context)
        {
            try
            {
                var providerEmailRequest = new ProviderEmailRequest
                {
                    TemplateId = message.Template,
                    Tokens = message.Tokens,
                    ExplicitEmailAddresses = string.IsNullOrWhiteSpace(message.EmailAddress)
                        ? new List<string>()
                        : new List<string> {message.EmailAddress}
                };

                var providerUsers = await _pasAccountApi.GetAccountUsers(message.ProviderId, CancellationToken.None);
               
                var finalRecipients = providerUsers
                    .Where(x => x.ReceiveNotifications)
                    .Select(x => x.EmailAddress)
                    .ToList();

                if (finalRecipients.Any())
                {
                    _logger.LogInformation($"Calling SendEmailCommand for {finalRecipients.Count()} emails");

                    var tasks = finalRecipients
                        .Select(email => context.Send(new SendEmailCommand(message.Template, email, message.Tokens)));

                    await Task.WhenAll(tasks);
                }
                else
                {
                    _logger.LogWarning($"No Email Addresses found to send Template {message.Template} for ProviderId {message.ProviderId}");
                }

            }
            catch (Exception e)
            {
                _logger.LogError($"Error processing {nameof(SendEmailToProviderCommand)}", e);
                throw;
            }
        }
    }
}