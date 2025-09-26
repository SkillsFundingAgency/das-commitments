using System;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class ApprenticeshipStoppedEvent
    {
        public long ApprenticeshipId { get; set; }
        public DateTime StopDate { get; set; }
        public DateTime AppliedOn { get; set; }
        public bool IsWithDrawnFromStart { get; set; }
        public long? LearnerDataId { get; set; }
        public long ProviderId { get; set; }   
    }
}
