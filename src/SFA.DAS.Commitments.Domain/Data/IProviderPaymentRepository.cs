using SFA.DAS.Commitments.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Domain.Data
{
    public interface IProviderPaymentRepository
    {
        Task<IList<ProviderPaymentPriorityItem>> GetCustomProviderPaymentPriority(long employerAccountId);
        Task UpdateProviderPaymentPriority(long employerAccountId, IList<ProviderPaymentPriorityUpdateItem> newPriorityList);
    }
}
