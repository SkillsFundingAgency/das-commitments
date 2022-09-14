using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Domain.Data
{
    public interface IDataLockRepository
    {
        Task<List<DataLockStatus>> GetExpirableDataLocks(DateTime beforeDate);
        Task<int> UpdateExpirableDataLocks(DataLockStatus dataLockStatus);
    }

}
