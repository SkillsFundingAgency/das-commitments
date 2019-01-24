using System;
using System.Collections.Generic;

namespace SFA.DAS.ProviderCommitments.Data.Models
{
    public class ConfirmedApprenticeship : Apprenticeship
    {
        public DateTime AgreedOn { get; set; }

        public virtual ICollection<DataLockStatus> DataLockStatus { get; set; }
        public virtual ICollection<PriceHistory> PriceHistory { get; set; }
    }
}