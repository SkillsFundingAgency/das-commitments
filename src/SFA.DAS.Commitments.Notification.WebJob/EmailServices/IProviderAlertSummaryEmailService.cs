using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Notification.WebJob.EmailServices
{
    public interface IProviderAlertSummaryEmailService
    {
        Task SendAlertSummaryEmails(string jobId);
    }
}