using System.Threading.Tasks;
using MediatR;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Application.Commands.AddTransferRequest;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class CohortTransferApprovalRequestedEventHandler : IHandleMessages<CohortTransferApprovalRequestedEvent>
    {
        private readonly IMediator _mediator;

        public CohortTransferApprovalRequestedEventHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Handle(CohortTransferApprovalRequestedEvent message, IMessageHandlerContext context)
        {
            var result = await _mediator.Send(new AddTransferRequestCommand { CohortId = message.CohortId });
        }
    }
}