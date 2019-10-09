using System;
using System.Threading.Tasks;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Commands;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers
{
    public class SendEmailCommandHandler : IHandleMessages<SendEmailCommand>
    {
        public Task Handle(SendEmailCommand message, IMessageHandlerContext context)
        {
            throw new NotImplementedException();
        }
    }
}
