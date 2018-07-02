using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Notifications.Api.Types;

namespace SFA.DAS.Commitments.Notification.WebJob.EmailServices
{
    public interface ISendingEmployerTransferRequestEmailService
    {
        Task<IEnumerable<Email>> GetEmails();
    }
}
