using SFA.DAS.CommitmentsV2.Application.Commands.UpdateAccountLegalEntityName;
using SFA.DAS.EmployerAccounts.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class UpdatedLegalEntityEventHandler(IMediator mediator) : IHandleMessages<UpdatedLegalEntityEvent>
{
    public Task Handle(UpdatedLegalEntityEvent message, IMessageHandlerContext context)
    {
        return mediator.Send(new UpdateAccountLegalEntityNameCommand(message.AccountLegalEntityId, message.Name, message.Created));
    }
}