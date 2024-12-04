using SFA.DAS.CommitmentsV2.Application.Commands.CreateAccount;
using SFA.DAS.EmployerAccounts.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class CreatedAccountEventHandler(IMediator mediator) : IHandleMessages<CreatedAccountEvent>
{
    public Task Handle(CreatedAccountEvent message, IMessageHandlerContext context)
    {
        return mediator.Send(new CreateAccountCommand(
            message.AccountId,
            message.HashedId,
            message.PublicHashedId,
            message.Name,
            message.Created)
        );
    }
}