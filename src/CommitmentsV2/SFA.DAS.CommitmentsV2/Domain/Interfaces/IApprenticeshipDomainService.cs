using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface IApprenticeshipDomainService
    {
        Task<List<EmployerAlertSummaryNotification>> GetEmployerAlertSummaryNotifications();
    }
}
