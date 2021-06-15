using System;
using System.Collections.Generic;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses
{
    public class GetDataLocksResponse
    {
        public IReadOnlyCollection<DataLock> DataLocks { get; set; }
    }
}
