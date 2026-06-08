using SFA.DAS.CommitmentsV2.MessageHandlers.Services;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class LearnerWithdrawalNotificationEventHandler(WithDrawalNotificationToEmployerService service, ILogger<LearnerWithdrawalNotificationEventHandler> logger) : IHandleMessages<LearnerWithdrawalNotificationEvent>
{
    public async Task Handle(LearnerWithdrawalNotificationEvent message, IMessageHandlerContext context)
    {       

        if (!message.IsWithdrawalFromIlr)
        {
            logger.LogInformation("{TypeName} received is not withdrawal from ILR for apprenticeship id {ApprenticeshipId}", nameof(LearnerWithdrawalNotificationEvent), message.ApprenticeshipId);
            return;
        }

        logger.LogInformation("Sending notification for {TypeName} received for apprenticeship id {ApprenticeshipId}", nameof(LearnerWithdrawalNotificationEvent), message.ApprenticeshipId);
        await service.SendWithdrawalNotificationToEmployer(message, context);
    }
}