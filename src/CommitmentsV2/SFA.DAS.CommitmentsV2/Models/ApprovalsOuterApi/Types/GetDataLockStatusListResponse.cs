using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types
{
    public class GetDataLockStatusListResponse
    {
        public int TotalNumberOfPages { get; set; }
        public List<DataLockStatus> DataLockStatuses { get; set; }
    }
}