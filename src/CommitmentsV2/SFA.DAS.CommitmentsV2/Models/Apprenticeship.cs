using System;
using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class Apprenticeship : ApprenticeshipBase
    {
        public virtual ICollection<DataLockStatus> DataLockStatus { get; set; }
        public virtual ICollection<PriceHistory> PriceHistory { get; set; }

        public int? PaymentOrder { get; set; }
        public DateTime? StopDate { get; set; }
        public DateTime? PauseDate { get; set; }
        public bool HasHadDataLockSuccess { get; set; }
        public byte? PendingUpdateOriginator { get; set; }


        public Apprenticeship()
        {
            DataLockStatus = new List<DataLockStatus>();
            PriceHistory = new List<PriceHistory>();
        }
    }
}