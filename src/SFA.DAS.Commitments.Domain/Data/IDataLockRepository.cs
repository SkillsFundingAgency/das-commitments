using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Entities.DataLock;

namespace SFA.DAS.Commitments.Domain.Data
{
    public interface IDataLockRepository
    {
        Task<long> GetLastDataLockEventId();
        Task<long> UpdateDataLockStatus(DataLockStatus dataLockStatus);
    }
}
