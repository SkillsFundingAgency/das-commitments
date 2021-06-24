using System.Threading.Tasks;
using MediatR;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Application.Commands.ApprenticeshipConfirmationCommenced;
using SFA.DAS.ApprenticeCommitments.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class ApprenticeshipConfirmationCommencedEventHandler : IHandleMessages<ApprenticeshipConfirmationCommencedEvent>
    {
        private readonly IMediator _mediator;

        public ApprenticeshipConfirmationCommencedEventHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task Handle(ApprenticeshipConfirmationCommencedEvent message, IMessageHandlerContext context)
        {
            return _mediator.Send(new ApprenticeshipConfirmationCommencedCommand(message.CommitmentsApprenticeshipId,
                message.CommitmentsApprovedOn, message.ConfirmationOverdueOn));
        }
    }
}