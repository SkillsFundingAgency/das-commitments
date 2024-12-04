using SFA.DAS.CommitmentsV2.Application.Commands.ApprenticeshipConfirmationCommenced;
using SFA.DAS.ApprenticeCommitments.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class ApprenticeshipConfirmationCommencedEventHandler(IMediator mediator, ILogger<ApprenticeshipConfirmationCommencedEventHandler> logger)
    : IHandleMessages<ApprenticeshipConfirmationCommencedEvent>
{
    public Task Handle(ApprenticeshipConfirmationCommencedEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("Message {TypeName} received, for commitments apprenticeship id {CommitmentsApprenticeshipId}", nameof(ApprenticeshipConfirmationCommencedEvent), message.CommitmentsApprenticeshipId);

        var command = new ApprenticeshipConfirmationCommencedCommand(
            message.CommitmentsApprenticeshipId,
            message.CommitmentsApprovedOn,
            message.ConfirmationOverdueOn
            );

        return mediator.Send(command);
    }
}