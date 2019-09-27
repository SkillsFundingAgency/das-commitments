using System;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class CohortFullyApprovedEvent
    {
        public long CohortId { get; }
        public DateTime UpdatedOn { get; }

        public CohortFullyApprovedEvent(long cohortId, DateTime updatedOn)
        {
            CohortId = cohortId;
            UpdatedOn = updatedOn;
        }
    }
}