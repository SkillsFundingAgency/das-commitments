using SFA.DAS.CommitmentsV2.Application.Commands.AddAccountLegalEntity;
using SFA.DAS.EmployerAccounts.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class AddedLegalEntityEventHandler : IHandleMessages<AddedLegalEntityEvent>
    {
        private readonly IMediator _mediator;

        public AddedLegalEntityEventHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task Handle(AddedLegalEntityEvent message, IMessageHandlerContext context)
        {
            return _mediator.Send(new AddAccountLegalEntityCommand(message.AccountId, message.AccountLegalEntityId, message.LegalEntityId,
                message.AccountLegalEntityPublicHashedId, message.OrganisationName, 
                (Models.OrganisationType)message.OrganisationType,
                message.OrganisationReferenceNumber,
                message.OrganisationAddress,
                message.Created));
        }
    }
}