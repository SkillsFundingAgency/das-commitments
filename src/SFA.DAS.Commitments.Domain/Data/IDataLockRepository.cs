using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using SFA.DAS.Commitments.Domain.Entities.DataLock;

namespace SFA.DAS.Commitments.Domain.Data
{
    public interface IDataLockRepository
    {
        Task<long> GetLastDataLockEventId();
        Task<long> UpdateDataLockStatus(DataLockStatus dataLockStatus);
        Task<List<DataLockStatus>> GetDataLocks(long apprenticeshipId, bool includeRemoved=false);
        Task<DataLockStatus> GetDataLock(long dataLockEventId);
        Task UpdateDataLockTriageStatus(long dataLockEventId, TriageStatus triageStatus);
        Task UpdateDataLockTriageStatus(IEnumerable<long> dataLockEventIds, TriageStatus triageStatus);
        Task ResolveDataLock(IEnumerable<long> dataLockEventIds);
        Task Delete(long dataLockEventId);
        Task<List<DataLockStatus>> GetExpirableDataLocks(DateTime beforeDate);
        Task<bool> UpdateExpirableDataLocks(long apprenticeshipId, string priceEpisodeIdentifier, DateTime expiredDateTime);
    }
    
}
