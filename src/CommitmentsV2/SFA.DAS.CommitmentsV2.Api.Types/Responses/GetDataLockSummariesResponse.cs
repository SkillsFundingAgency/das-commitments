using SFA.DAS.CommitmentsV2.Types;
using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses;

public class GetDataLockSummariesResponse
{
    public IReadOnlyCollection<DataLock> DataLocksWithCourseMismatch { get; set; }
    public IReadOnlyCollection<DataLock> DataLocksWithOnlyPriceMismatch { get; set; }
}