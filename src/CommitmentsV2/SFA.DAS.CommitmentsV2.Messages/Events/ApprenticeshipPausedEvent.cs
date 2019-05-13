using System;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class ApprenticeshipPausedEvent
    {
        public long ApprenticeshipId { get; set; }
        public DateTime PausedOn { get; set; }
    }
}
