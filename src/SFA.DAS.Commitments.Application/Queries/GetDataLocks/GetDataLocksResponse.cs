using System.Collections.Generic;

using SFA.DAS.Commitments.Domain.Entities.DataLock;

namespace SFA.DAS.Commitments.Application.Queries.GetDataLocks
{
    public class GetDataLocksResponse : QueryResponse<IList<DataLockStatus>>
    {
    }
}
