using MediatR;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class ChangeOfPartyRequestCreatedEventHandlerForEmail : IHandleMessages<ChangeOfPartyRequestCreatedEvent>
    {
        public const string TemplateProviderChangeOfProviderRequested = "ProviderApprenticeshipChangeOfProviderRequested_dev";

        private readonly IMediator _mediator;

        public ChangeOfPartyRequestCreatedEventHandlerForEmail(IMediator mediator)
        {
            _mediator = mediator;
        }
        public Task Handle(ChangeOfPartyRequestCreatedEvent message, IMessageHandlerContext context)
        {
            throw new NotImplementedException();
        }
    }
}
