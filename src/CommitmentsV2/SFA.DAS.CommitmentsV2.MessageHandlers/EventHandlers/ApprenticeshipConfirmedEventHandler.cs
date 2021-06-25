using System.Threading.Tasks;
using MediatR;
using NServiceBus;
using SFA.DAS.ApprenticeCommitments.Messages.Events;
using SFA.DAS.CommitmentsV2.Application.Commands.ApprenticeshipConfirmed;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class ApprenticeshipConfirmedEventHandler : IHandleMessages<ApprenticeshipConfirmationConfirmedEvent>
    {
        private readonly IMediator _mediator;

        public ApprenticeshipConfirmedEventHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task Handle(ApprenticeshipConfirmationConfirmedEvent message, IMessageHandlerContext context)
        {
            return _mediator.Send(new ApprenticeshipConfirmedCommand(message.CommitmentsApprenticeshipId,
                message.CommitmentsApprovedOn, message.ConfirmedOn));
        }
    }
}