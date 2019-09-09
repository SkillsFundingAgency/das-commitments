using System;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class ApprovedCohortReturnedToProviderEvent
    {
        public long CohortId { get; }
        public DateTime UpdatedOn { get; }

        public ApprovedCohortReturnedToProviderEvent(long cohortId, DateTime updatedOn)
        {
            CohortId = cohortId;
            UpdatedOn = updatedOn;
        }
    }
}