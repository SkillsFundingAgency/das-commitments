using SFA.DAS.CommitmentsV2.MessageHandlers.Services.Interface;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class ApprenticeshipStopDateChangedEventHandler(IWithDrawalNotificationToEmployerService service, ILogger<ApprenticeshipStopDateChangedEventHandler> logger) : IHandleMessages<ApprenticeshipStopDateChangedEvent>
{
    public async Task Handle(ApprenticeshipStopDateChangedEvent message, IMessageHandlerContext context)
    {
        if (!message.IsWithdrawnViaIlr)
        {
            logger.LogInformation("LearnerStoppedDateUpdatedNotificationEventHandler received is not withdrawal from ILR for apprenticeship id {ApprenticeshipId}", message.ApprenticeshipId);
            return;
        }

        logger.LogInformation("Sending notification for LearnerStoppedDateUpdatedNotificationEventHandler received for apprenticeship id {ApprenticeshipId}", message.ApprenticeshipId);
        await service.SendWithdrawalNotificationToEmployer(message.ApprenticeshipId, context);
    }
}