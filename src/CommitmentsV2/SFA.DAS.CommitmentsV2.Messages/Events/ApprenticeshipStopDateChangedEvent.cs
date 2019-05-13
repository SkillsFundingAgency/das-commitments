using System;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class ApprenticeshipStopDateChangedEvent
    {
        public long ApprenticeshipId { get; set; }
        public DateTime StopDate { get; set; }
        public DateTime ChangedOn { get; set; }
    }
}
