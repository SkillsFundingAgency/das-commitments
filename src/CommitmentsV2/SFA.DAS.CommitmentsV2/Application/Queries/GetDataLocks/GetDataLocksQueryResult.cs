using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetDataLocks;

public class GetDataLocksQueryResult
{
    public IReadOnlyCollection<DataLock> DataLocks { get; set; }
}