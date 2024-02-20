using SFA.DAS.ApprenticeCommitments.Messages.Events;
using SFA.DAS.CommitmentsV2.Application.Commands.ApprenticeshipEmailAddressConfirmed;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class ApprenticeshipEmailAddressConfirmedEventHandler : IHandleMessages<ApprenticeshipEmailAddressConfirmedEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ApprenticeshipEmailAddressConfirmedEventHandler> _logger;

        public ApprenticeshipEmailAddressConfirmedEventHandler(IMediator mediator, ILogger<ApprenticeshipEmailAddressConfirmedEventHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public Task Handle(ApprenticeshipEmailAddressConfirmedEvent message, IMessageHandlerContext context)
        {
            _logger.LogInformation($"Message {nameof(ApprenticeshipEmailAddressConfirmedEvent)} received, for commitments apprenticeship id {message.CommitmentsApprenticeshipId}");
            return _mediator.Send(new ApprenticeshipEmailAddressConfirmedCommand(message.ApprenticeId, message.CommitmentsApprenticeshipId));
        }
    }
}