using System;

namespace SFA.DAS.CommitmentsV2.Models
{
    public partial class PriceHistory
    {
        public long Id { get; set; }
        public long ApprenticeshipId { get; set; }
        public decimal Cost { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public virtual ConfirmedApprenticeship ConfirmedApprenticeship { get; set; }
    }
}
