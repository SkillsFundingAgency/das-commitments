using SFA.DAS.CommitmentsV2.Models;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface IAlertSummaryService
    {
        Task SendEmployerAlertSummaryNotifications();
    }
}
