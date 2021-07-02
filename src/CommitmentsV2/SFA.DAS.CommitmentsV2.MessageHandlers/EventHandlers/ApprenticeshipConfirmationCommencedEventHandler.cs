using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Application.Commands.ApprenticeshipConfirmationCommenced;
using SFA.DAS.ApprenticeCommitments.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class ApprenticeshipConfirmationCommencedEventHandler : IHandleMessages<ApprenticeshipConfirmationCommencedEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ApprenticeshipConfirmationCommencedEventHandler> _logger;

        public ApprenticeshipConfirmationCommencedEventHandler(IMediator mediator, ILogger<ApprenticeshipConfirmationCommencedEventHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public Task Handle(ApprenticeshipConfirmationCommencedEvent message, IMessageHandlerContext context)
        {
            _logger.LogInformation($"Message {nameof(ApprenticeshipConfirmationCommencedEvent)} received, for commitments apprenticeship id {message.CommitmentsApprenticeshipId}");
            return _mediator.Send(new ApprenticeshipConfirmationCommencedCommand(message.CommitmentsApprenticeshipId,
                message.CommitmentsApprovedOn, message.ConfirmationOverdueOn));
        }
    }
}