using System;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class CohortTransferApprovalRequestedEvent
    {
        public long CohortId { get; }
        public DateTime UpdatedOn { get; }

        public CohortTransferApprovalRequestedEvent(long cohortId, DateTime updatedOn)
        {
            CohortId = cohortId;
            UpdatedOn = updatedOn;
        }
    }
}