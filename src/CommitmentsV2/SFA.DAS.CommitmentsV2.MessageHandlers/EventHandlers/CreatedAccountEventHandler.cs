using System.Threading.Tasks;
using MediatR;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Application.Commands.CreateAccount;
using SFA.DAS.EmployerAccounts.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class CreatedAccountEventHandler : IHandleMessages<CreatedAccountEvent>
    {
        private readonly IMediator _mediator;

        public CreatedAccountEventHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task Handle(CreatedAccountEvent message, IMessageHandlerContext context)
        {
            return _mediator.Send(new CreateAccountCommand(message.AccountId, message.HashedId, message.PublicHashedId,
                message.Name, message.Created));
        }
    }
}
