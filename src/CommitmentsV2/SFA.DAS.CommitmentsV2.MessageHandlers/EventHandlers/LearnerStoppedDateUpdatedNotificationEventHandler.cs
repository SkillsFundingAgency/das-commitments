using SFA.DAS.CommitmentsV2.MessageHandlers.Services.Interface;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class LearnerStoppedDateUpdatedNotificationEventHandler(IWithDrawalNotificationToEmployerService service, ILogger<LearnerStoppedDateUpdatedNotificationEventHandler> logger) : IHandleMessages<LearnerWithdrawalNotificationEvent>
{
    public async Task Handle(LearnerWithdrawalNotificationEvent message, IMessageHandlerContext context)
    {       

        if (!message.IsWithdrawalFromIlr)
        {
            logger.LogInformation("LearnerStoppedDateUpdatedNotificationEventHandler received is not withdrawal from ILR for apprenticeship id {ApprenticeshipId}", message.ApprenticeshipId);
            return;
        }

        logger.LogInformation("Sending notification for LearnerStoppedDateUpdatedNotificationEventHandler received for apprenticeship id {ApprenticeshipId}", message.ApprenticeshipId);
        await service.SendWithdrawalNotificationToEmployer(message, context);
    }
}