using SFA.DAS.CommitmentsV2.Application.Commands.RemoveAccountLegalEntity;
using SFA.DAS.EmployerAccounts.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class RemovedLegalEntityEventHandler(IMediator mediator) : IHandleMessages<RemovedLegalEntityEvent>
{
    public Task Handle(RemovedLegalEntityEvent message, IMessageHandlerContext context)
    {
        return mediator.Send(new RemoveAccountLegalEntityCommand(message.AccountId, message.AccountLegalEntityId, message.Created));
    }
}