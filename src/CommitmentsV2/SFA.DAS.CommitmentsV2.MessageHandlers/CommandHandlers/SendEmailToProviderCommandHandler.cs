using System.Collections.Generic;
using System.Threading.Tasks;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.PAS.Account.Api.ClientV2;
using SFA.DAS.PAS.Account.Api.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers
{
    public class SendEmailToProviderCommandHandler : IHandleMessages<SendEmailToProviderCommand>
    {
        private readonly IPasAccountApiClient _pasAccountApi;

        public SendEmailToProviderCommandHandler(IPasAccountApiClient pasAccountApi)
        {
            _pasAccountApi = pasAccountApi;
        }

        public async Task Handle(SendEmailToProviderCommand message, IMessageHandlerContext context)
        {
            var providerEmailRequest = new ProviderEmailRequest
            {
                TemplateId = message.Template,
                Tokens = message.Tokens,
                ExplicitEmailAddresses = string.IsNullOrWhiteSpace(message.EmailAddress)
                    ? new List<string>()
                    : new List<string> { message.EmailAddress }
            };

            await _pasAccountApi.SendEmailToAllProviderRecipients(message.ProviderId, providerEmailRequest);
        }
    }
}