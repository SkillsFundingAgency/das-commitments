using System;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class HistoryItemV2
    {
        public long Id { get; set; }
        public Guid TransactionId { get; set; }
        public string EntityType { get; set; }
        public string EntityState { get; set; }
        public string Original { get; set; }
        public string Modified { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}