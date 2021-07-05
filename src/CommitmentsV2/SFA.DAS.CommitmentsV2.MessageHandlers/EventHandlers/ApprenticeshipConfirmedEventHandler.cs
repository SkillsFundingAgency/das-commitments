using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.ApprenticeCommitments.Messages.Events;
using SFA.DAS.CommitmentsV2.Application.Commands.ApprenticeshipConfirmed;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class ApprenticeshipConfirmedEventHandler : IHandleMessages<ApprenticeshipConfirmationConfirmedEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ApprenticeshipConfirmedEventHandler> _logger;

        public ApprenticeshipConfirmedEventHandler(IMediator mediator, ILogger<ApprenticeshipConfirmedEventHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public Task Handle(ApprenticeshipConfirmationConfirmedEvent message, IMessageHandlerContext context)
        {
            _logger.LogInformation($"Message {nameof(ApprenticeshipConfirmationConfirmedEvent)} received, for commitments apprenticeship id {message.CommitmentsApprenticeshipId}");
            return _mediator.Send(new ApprenticeshipConfirmedCommand(message.CommitmentsApprenticeshipId,
                message.CommitmentsApprovedOn, message.ConfirmedOn));
        }
    }
}