using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.ApprenticeCommitments.Messages.Events;
using SFA.DAS.CommitmentsV2.Application.Commands.ApprenticeshipEmailAddressChangedByApprentice;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class ApprenticeshipEmailAddressChangedEventHandler : IHandleMessages<ApprenticeshipEmailAddressChangedEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ApprenticeshipEmailAddressChangedEventHandler> _logger;

        public ApprenticeshipEmailAddressChangedEventHandler(IMediator mediator, ILogger<ApprenticeshipEmailAddressChangedEventHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public Task Handle(ApprenticeshipEmailAddressChangedEvent message, IMessageHandlerContext context)
        {
            _logger.LogInformation($"Message {nameof(ApprenticeshipEmailAddressChangedEvent)} received, for commitments apprenticeship id {message.CommitmentsApprenticeshipId}");
            return _mediator.Send(new ApprenticeshipEmailAddressChangedByApprenticeCommand(message.ApprenticeId, message.CommitmentsApprenticeshipId));
        }
    }
}