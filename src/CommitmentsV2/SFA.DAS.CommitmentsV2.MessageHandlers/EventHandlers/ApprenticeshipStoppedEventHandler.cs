using SFA.DAS.CommitmentsV2.MessageHandlers.Services.Interface;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class ApprenticeshipStoppedEventHandler(IWithDrawalNotificationToEmployerService service, ILogger<ApprenticeshipStoppedEventHandler> logger) : IHandleMessages<ApprenticeshipStoppedEvent>
{
    public async Task Handle(ApprenticeshipStoppedEvent message, IMessageHandlerContext context)
    {
        if (!message.IsWithdrawnViaIlr)
        {
            logger.LogInformation("{TypeName} received is not withdrawal from ILR for apprenticeship id {ApprenticeshipId}", nameof(ApprenticeshipStoppedEventHandler), message.ApprenticeshipId);
            return;
        }

        logger.LogInformation("Sending notification for {TypeName} received for apprenticeship id {ApprenticeshipId}", nameof(ApprenticeshipStoppedEventHandler), message.ApprenticeshipId);
        await service.SendWithdrawalNotificationToEmployer(message.ApprenticeshipId, context);
    }
}