using SFA.DAS.CommitmentsV2.Application.Commands.UpdateAccountLegalEntityName;
using SFA.DAS.EmployerAccounts.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class UpdatedLegalEntityEventHandler : IHandleMessages<UpdatedLegalEntityEvent>
    {
        private readonly IMediator _mediator;

        public UpdatedLegalEntityEventHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task Handle(UpdatedLegalEntityEvent message, IMessageHandlerContext context)
        {
            return _mediator.Send(new UpdateAccountLegalEntityNameCommand(message.AccountLegalEntityId, message.Name, message.Created));
        }
    }
}