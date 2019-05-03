using System;
using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class ApprovedApprenticeship : Apprenticeship
    {
        public DateTime AgreedOn { get; set; }

        public virtual ICollection<DataLockStatus> DataLockStatus { get; set; }
        public virtual ICollection<PriceHistory> PriceHistory { get; set; }
    }
}