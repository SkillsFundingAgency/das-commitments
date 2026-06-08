using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.Services.Interface;

public interface IWithDrawalNotificationToEmployerService
{
    Task SendWithdrawalNotificationToEmployer(LearnerWithdrawalNotificationEvent message, IMessageHandlerContext context);
}
