using System;

namespace SFA.DAS.CommitmentsV2.Models
{
    public partial class DataLockStatus
    {
        public long Id { get; set; }
        public long DataLockEventId { get; set; }
        public DateTime DataLockEventDatetime { get; set; }
        public string PriceEpisodeIdentifier { get; set; }
        public long ApprenticeshipId { get; set; }
        public string IlrTrainingCourseCode { get; set; }
        public byte IlrTrainingType { get; set; }
        public DateTime? IlrActualStartDate { get; set; }
        public DateTime? IlrEffectiveFromDate { get; set; }
        public DateTime? IlrPriceEffectiveToDate { get; set; }
        public decimal? IlrTotalCost { get; set; }
        public int ErrorCode { get; set; }
        public byte Status { get; set; }
        public byte TriageStatus { get; set; }
        public long? ApprenticeshipUpdateId { get; set; }
        public bool IsResolved { get; set; }
        public byte EventStatus { get; set; }
        public bool IsExpired { get; set; }
        public DateTime? Expired { get; set; }

        public virtual ConfirmedApprenticeship ConfirmedApprenticeship { get; set; }
        public virtual ApprenticeshipUpdate ApprenticeshipUpdate { get; set; }
    }
}
