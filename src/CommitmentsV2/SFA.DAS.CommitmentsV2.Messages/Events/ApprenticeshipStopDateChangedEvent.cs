using System;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class ApprenticeshipStopDateChangedEvent
    {
        long ApprenticeshipId { get; }
        DateTime StopDate { get; }
        DateTime ChangedOn { get; }

        public ApprenticeshipStopDateChangedEvent(long apprenticeshipId, DateTime stopDate, DateTime changedOn)
        {
            ApprenticeshipId = apprenticeshipId;
            StopDate = stopDate;
            ChangedOn = changedOn;
        }
    }
}
