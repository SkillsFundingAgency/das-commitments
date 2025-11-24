using System;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class ApprenticeshipStopDateChangedEvent
    {
        public long ApprenticeshipId { get; set; }
        public DateTime StopDate { get; set; }
        public DateTime ChangedOn { get; set; }
        public bool IsWithDrawnAtStartOfCourse { get; set; }
        public long? LearnerDataId { get; set; }
        public long ProviderId { get; set; }
    }
}
