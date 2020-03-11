using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Commands;
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

                await _pasAccountApi.SendEmailToAllProviderRecipients(message.ProviderId, providerEmailRequest);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error processing {nameof(SendEmailToProviderCommand)}", e);
                throw;
            }
        }
    }
}