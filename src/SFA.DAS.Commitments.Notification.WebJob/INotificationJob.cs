using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Notification.WebJob
{
    public interface INotificationJob
    {
        Task RunEmployerAlertSummaryNotification(string jobId);

        Task RunProviderAlertSummaryNotification(string jobId);

        Task RunSendingEmployerTransferRequestNotification(string jobId);
    }
}