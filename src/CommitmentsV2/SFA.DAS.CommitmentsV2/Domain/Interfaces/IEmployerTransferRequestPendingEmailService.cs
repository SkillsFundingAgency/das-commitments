namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface IEmployerTransferRequestPendingEmailService
    {
        Task SendEmployerTransferRequestPendingNotifications();
    }
}
