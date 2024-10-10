using SFA.DAS.ApprenticeCommitments.Messages.Events;
using SFA.DAS.CommitmentsV2.Application.Commands.ApprenticeshipEmailAddressConfirmed;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class ApprenticeshipEmailAddressConfirmedEventHandler(IMediator mediator, ILogger<ApprenticeshipEmailAddressConfirmedEventHandler> logger)
    : IHandleMessages<ApprenticeshipEmailAddressConfirmedEvent>
{
    public Task Handle(ApprenticeshipEmailAddressConfirmedEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("Message {TypeName} received, for commitments apprenticeship id {CommitmentsApprenticeshipId}", nameof(ApprenticeshipEmailAddressConfirmedEvent), message.CommitmentsApprenticeshipId);

        var command = new ApprenticeshipEmailAddressConfirmedCommand(message.ApprenticeId, message.CommitmentsApprenticeshipId);
        
        return mediator.Send(command);
    }
}