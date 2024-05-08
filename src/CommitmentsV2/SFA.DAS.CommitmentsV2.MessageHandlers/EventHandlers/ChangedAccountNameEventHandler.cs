using SFA.DAS.CommitmentsV2.Application.Commands.UpdateAccountName;
using SFA.DAS.EmployerAccounts.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class ChangedAccountNameEventHandler : IHandleMessages<ChangedAccountNameEvent>
    {
        private readonly IMediator _mediator;

        public ChangedAccountNameEventHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task Handle(ChangedAccountNameEvent message, IMessageHandlerContext context)
        {
            return _mediator.Send(new UpdateAccountNameCommand(message.AccountId, message.CurrentName, message.Created));
        }
    }
}