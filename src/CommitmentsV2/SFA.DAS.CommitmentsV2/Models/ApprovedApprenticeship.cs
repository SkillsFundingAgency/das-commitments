using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class ApprovedApprenticeship : Apprenticeship
    {
        public virtual ICollection<DataLockStatus> DataLockStatus { get; set; }
        public virtual ICollection<PriceHistory> PriceHistory { get; set; }

        public ApprovedApprenticeship()
        {
            DataLockStatus = new List<DataLockStatus>();
            PriceHistory = new List<PriceHistory>();
        }
    }
}