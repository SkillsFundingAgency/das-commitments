using System;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class ApprenticeshipWithChangeOfPartyCreatedEvent
    {
        public long ApprenticeshipId { get; }
        public long ChangeOfPartyRequestId { get; }
        public DateTime CreatedOn { get; }

        public ApprenticeshipWithChangeOfPartyCreatedEvent(long apprenticeshipId, long changeOfPartyRequestId, DateTime createdOn)
        {
            ApprenticeshipId = apprenticeshipId;
            ChangeOfPartyRequestId = changeOfPartyRequestId;
            CreatedOn = createdOn;
        }
    }
}
