using SFA.DAS.ApprenticeCommitments.Messages.Events;
using SFA.DAS.CommitmentsV2.Application.Commands.ApprenticeshipConfirmed;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class ApprenticeshipConfirmedEventHandler(IMediator mediator, ILogger<ApprenticeshipConfirmedEventHandler> logger)
    : IHandleMessages<ApprenticeshipConfirmationConfirmedEvent>
{
    public Task Handle(ApprenticeshipConfirmationConfirmedEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("Message {TypeName} received, for commitments apprenticeship id {CommitmentsApprenticeshipId}", nameof(ApprenticeshipConfirmationConfirmedEvent), message.CommitmentsApprenticeshipId);

        var command = new ApprenticeshipConfirmedCommand(
            message.CommitmentsApprenticeshipId,
            message.CommitmentsApprovedOn,
            message.ConfirmedOn
        );

        return mediator.Send(command);
    }
}