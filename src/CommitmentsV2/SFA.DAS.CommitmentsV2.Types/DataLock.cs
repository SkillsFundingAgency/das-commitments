using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.CommitmentsV2.Types
{
    public class DataLock
    {
        public long Id { get; set; }
        public DateTime DataLockEventDatetime { get; set; }
        public string PriceEpisodeIdentifier { get; set; }
        public long ApprenticeshipId { get; set; }
        public string IlrTrainingCourseCode { get; set; }
        public DateTime? IlrActualStartDate { get; set; }
        public DateTime? IlrEffectiveFromDate { get; set; }
        public DateTime? IlrPriceEffectiveToDate { get; set; }
        public decimal? IlrTotalCost { get; set; }
        public DataLockErrorCode ErrorCode { get; set; }
        public Status DataLockStatus { get; set; }
        public TriageStatus TriageStatus { get; set; }
        public bool IsResolved { get; set; }
    }
}
