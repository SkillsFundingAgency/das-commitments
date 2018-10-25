using System;

namespace SFA.DAS.Commitments.Domain.Entities
{
    public class PriceHistory
    {
        public long Id { get; set; }
        public long ApprenticeshipId { get; set; }

        public decimal Cost { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime? ToDate { get; set; }
    }
}