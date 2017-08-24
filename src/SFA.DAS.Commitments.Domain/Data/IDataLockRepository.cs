using System.Collections.Generic;
using System.Threading.Tasks;

using SFA.DAS.Commitments.Domain.Entities.DataLock;

namespace SFA.DAS.Commitments.Domain.Data
{
    public interface IDataLockRepository
    {
        Task<long> GetLastDataLockEventId();
        Task<long> UpdateDataLockStatus(DataLockStatus dataLockStatus);
        Task<List<DataLockStatus>> GetDataLocks(long apprenticeshipId);
        Task<DataLockStatus> GetDataLock(long dataLockEventId);
        Task<long> UpdateDataLockTriageStatus(long dataLockEventId, TriageStatus triageStatus);
        Task<long> UpdateDataLockTriageStatus(IEnumerable<long> dataLockEventIds, TriageStatus triageStatus);
        Task<long> ResolveDataLock(IEnumerable<long> dataLockEventIds);
        Task Delete(long dataLockEventId);
    }
}
