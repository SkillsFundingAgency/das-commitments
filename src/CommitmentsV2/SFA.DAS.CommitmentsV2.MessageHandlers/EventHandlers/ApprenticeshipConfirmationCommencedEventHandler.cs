using System;
using System.Threading.Tasks;
using MediatR;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Application.Commands.ApprenticeshipConfirmationCommenced;

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
                message.CommitmentsApprovedOn, message.ApprenticeshipConfirmationOverdueOn));
        }
    }

    public class ApprenticeshipConfirmationCommencedEvent
    {
        public Guid ApprenticeId {get; set;}
        public long? ADCApprenticeshipId {get; set;}
        public long? ApprenticeshipConfirmationId { get; set; }
        public DateTime ApprenticeshipConfirmationOverdueOn { get; set; }
        public long CommitmentsApprenticeshipId { get; set; }
        public DateTime CommitmentsApprovedOn { get; set; }
    }
}