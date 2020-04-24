using System;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class CohortWithChangeOfPartyCreatedEvent
    {
        public long CohortId { get; }
        public long ChangeOfPartyRequestId { get; }
        public DateTime CreatedOn { get; }

        public CohortWithChangeOfPartyCreatedEvent(long cohortId, long changeOfPartyRequestId, DateTime createdOn)
        {
            CohortId = cohortId;
            ChangeOfPartyRequestId = changeOfPartyRequestId;
            CreatedOn = createdOn;
        }
    }
}
