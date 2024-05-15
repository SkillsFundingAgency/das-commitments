using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetDataLockSummaries
{
    public class GetDataLockSummariesQueryResult
    {
        public IReadOnlyCollection<DataLock> DataLocksWithCourseMismatch { get; set; }
        public IReadOnlyCollection<DataLock> DataLocksWithOnlyPriceMismatch { get; set; }
    }
}
