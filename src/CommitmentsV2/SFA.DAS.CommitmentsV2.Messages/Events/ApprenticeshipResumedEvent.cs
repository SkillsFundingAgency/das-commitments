using System;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class ApprenticeshipResumedEvent
    {
        public long ApprenticeshipId { get; set; }
        public DateTime ResumedOn { get; set; }
    }
}
