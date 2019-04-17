using System;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public interface IDraftApprenticeshipStoppedEvent
    {
        long ApprenticeshipId { get; set; }
        DateTime StopDate { get; set; }
        DateTime AppliedOn { get; set; }
    }
}
