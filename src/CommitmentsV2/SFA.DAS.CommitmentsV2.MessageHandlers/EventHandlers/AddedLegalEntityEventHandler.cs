using System.Threading.Tasks;
using MediatR;
using NServiceBus;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.ProviderRelationships.Application.Commands.AddAccountLegalEntity;

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
            return _mediator.Send(new AddAccountLegalEntityCommand(message.AccountId, message.AccountLegalEntityId, message.AccountLegalEntityPublicHashedId, message.OrganisationName, message.Created));
        }
    }
}