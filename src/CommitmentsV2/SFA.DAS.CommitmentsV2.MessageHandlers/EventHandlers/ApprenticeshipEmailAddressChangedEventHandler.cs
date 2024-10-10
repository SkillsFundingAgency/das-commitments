using SFA.DAS.ApprenticeCommitments.Messages.Events;
using SFA.DAS.CommitmentsV2.Application.Commands.ApprenticeshipEmailAddressChangedByApprentice;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class ApprenticeshipEmailAddressChangedEventHandler(IMediator mediator, ILogger<ApprenticeshipEmailAddressChangedEventHandler> logger)
    : IHandleMessages<ApprenticeshipEmailAddressChangedEvent>
{
    public Task Handle(ApprenticeshipEmailAddressChangedEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("Message {TypeName} received, for commitments apprenticeship id {CommitmentsApprenticeshipId}", nameof(ApprenticeshipEmailAddressChangedEvent), message.CommitmentsApprenticeshipId);

        var command = new ApprenticeshipEmailAddressChangedByApprenticeCommand(message.ApprenticeId, message.CommitmentsApprenticeshipId);
        
        return mediator.Send(command);
    }
}