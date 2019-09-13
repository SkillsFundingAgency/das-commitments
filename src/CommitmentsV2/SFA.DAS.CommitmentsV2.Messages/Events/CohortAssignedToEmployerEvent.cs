using System;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class CohortAssignedToEmployerEvent
    {
        public long CohortId { get; }
        public DateTime UpdatedOn { get; }

        public CohortAssignedToEmployerEvent(long cohortId, DateTime updatedOn)
        {
            CohortId = cohortId;
            UpdatedOn = updatedOn;
        }
    }
}