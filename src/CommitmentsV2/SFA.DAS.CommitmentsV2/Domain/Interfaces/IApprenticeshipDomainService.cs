using SFA.DAS.CommitmentsV2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface IApprenticeshipDomainService
    {
        Task<List<EmployerAlertSummaryNotification>> GetEmployerAlertSummaryNotifications();
    }
}
