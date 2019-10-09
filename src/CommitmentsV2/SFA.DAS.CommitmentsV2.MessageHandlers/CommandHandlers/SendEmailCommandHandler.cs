using System.Threading.Tasks;
using MoreLinq.Extensions;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.Notifications.Api.Client;
using SFA.DAS.Notifications.Api.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers
{
    public class SendEmailCommandHandler : IHandleMessages<SendEmailCommand>
    {
        private readonly INotificationsApi _notificationsApiClient;

        public SendEmailCommandHandler(INotificationsApi notificationsApiClient)
        {
            _notificationsApiClient = notificationsApiClient;
        }

        public async Task Handle(SendEmailCommand message, IMessageHandlerContext context)
        {
            var email = new Email("x",
                message.TemplateId,
                "x",
                message.RecipientsAddress,
                message.ReplyToAddress,
                message.Tokens.ToDictionary());

            await _notificationsApiClient.SendEmail(email);
        }
    }
}
