using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Notification.WebJob
{
    public interface INotificationJob
    {
        Task RunEmployerNotification(string jobId);

        Task RunProviderNotification(string jobId);
    }
}