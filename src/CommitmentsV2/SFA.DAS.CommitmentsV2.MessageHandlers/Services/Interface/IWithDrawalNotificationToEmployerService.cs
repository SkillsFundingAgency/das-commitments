namespace SFA.DAS.CommitmentsV2.MessageHandlers.Services.Interface;

public interface IWithDrawalNotificationToEmployerService
{
    Task SendWithdrawalNotificationToEmployer(long apprenticeshipId, IMessageHandlerContext context);
}