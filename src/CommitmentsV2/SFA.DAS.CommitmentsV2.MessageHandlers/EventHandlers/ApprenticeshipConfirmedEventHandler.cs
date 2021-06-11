using System;
using System.Threading.Tasks;
using MediatR;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Application.Commands.ApprenticeshipConfirmed;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class ApprenticeshipConfirmedEventHandler : IHandleMessages<ApprenticeshipConfirmedEvent>
    {
        private readonly IMediator _mediator;

        public ApprenticeshipConfirmedEventHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task Handle(ApprenticeshipConfirmedEvent message, IMessageHandlerContext context)
        {
            return _mediator.Send(new ApprenticeshipConfirmedCommand(message.CommitmentsApprenticeshipId,
                message.CommitmentsApprovedOn, message.ApprenticeshipConfirmedOn));
        }
    }

    public class ApprenticeshipConfirmedEvent
    {
        public Guid ApprenticeId {get; set;}
        public long? ADCApprenticeshipId {get; set;}
        public long? ApprenticeshipConfirmationId { get; set; }
        public DateTime ApprenticeshipConfirmedOn { get; set; }
        public long CommitmentsApprenticeshipId { get; set; }
        public DateTime CommitmentsApprovedOn { get; set; }
    }
}