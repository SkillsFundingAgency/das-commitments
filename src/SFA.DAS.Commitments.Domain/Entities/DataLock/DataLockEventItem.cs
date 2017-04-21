using System;
using System.Collections.Generic;

namespace SFA.DAS.Commitments.Domain.Entities.DataLock
{
    public class DataLockEventItem
    {
        public long DataLockEventId { get; set; }
        public DateTime DataLockEventDatetime { get; set; }
        public string PriceEpisodeIdentifier { get; set; }
        public long ApprenticeshipId { get; set; }
        public string IlrTrainingCourseCode { get; set; }
        public TrainingType IlrTrainingType { get; set; }
        public DateTime? IlrActualStartDate { get; set; }
        public DateTime? IlrEffectiveFromDate { get; set; }
        public decimal? IlrTotalCost { get; set; }
        public IEnumerable<DataLockEventError> ErrorCodes { get; set; }
        public DataLockStatus DataLockStatus { get; set; }
        public TriageStatus TriageStatus { get; set; }
    }
}
