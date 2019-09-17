using System;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class CohortAssignedToProviderEvent
    {
        public long CohortId { get; }
        public DateTime UpdatedOn { get; }

        public CohortAssignedToProviderEvent(long cohortId, DateTime updatedOn)
        {
            CohortId = cohortId;
            UpdatedOn = updatedOn;
        }
    }
}