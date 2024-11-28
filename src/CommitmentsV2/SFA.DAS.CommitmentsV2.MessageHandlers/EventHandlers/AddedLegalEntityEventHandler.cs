using SFA.DAS.CommitmentsV2.Application.Commands.AddAccountLegalEntity;
using SFA.DAS.EmployerAccounts.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class AddedLegalEntityEventHandler(IMediator mediator) : IHandleMessages<AddedLegalEntityEvent>
{
    public Task Handle(AddedLegalEntityEvent message, IMessageHandlerContext context)
    {
        return mediator.Send(new AddAccountLegalEntityCommand(message.AccountId, message.AccountLegalEntityId, message.LegalEntityId,
            message.AccountLegalEntityPublicHashedId, message.OrganisationName, 
            (Models.OrganisationType)message.OrganisationType,
            message.OrganisationReferenceNumber,
            message.OrganisationAddress,
            message.Created));
    }
}