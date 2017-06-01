using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Notification.WebJob
{
    public interface INotificationJob
    {
        Task Run();
    }
}