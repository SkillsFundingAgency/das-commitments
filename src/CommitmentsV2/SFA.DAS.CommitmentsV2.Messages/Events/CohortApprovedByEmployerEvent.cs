using System;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class CohortApprovedByEmployerEvent
    {
        public long CohortId { get; }
        public DateTime UpdatedOn { get; }

        public CohortApprovedByEmployerEvent(long cohortId, DateTime updatedOn)
        {
            CohortId = cohortId;
            UpdatedOn = updatedOn;
        }
    }
}