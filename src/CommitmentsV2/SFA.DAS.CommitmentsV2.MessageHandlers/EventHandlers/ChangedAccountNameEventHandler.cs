using SFA.DAS.CommitmentsV2.Application.Commands.UpdateAccountName;
using SFA.DAS.EmployerAccounts.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class ChangedAccountNameEventHandler(IMediator mediator) : IHandleMessages<ChangedAccountNameEvent>
{
    public Task Handle(ChangedAccountNameEvent message, IMessageHandlerContext context)
    {
        return mediator.Send(new UpdateAccountNameCommand(message.AccountId, message.CurrentName, message.Created));
    }
}