using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types
{
    public class GetDataLockStatusListResponse
    {
        public IEnumerable<DataLockStatus> DataLockStatuses { get; set; }
    }
}