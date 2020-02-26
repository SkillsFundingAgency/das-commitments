using System;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class CohortAssignedToEmployerEvent
    {
        public long CohortId { get; }
        public DateTime UpdatedOn { get; }
        public Party AssignedBy { get; }

        public CohortAssignedToEmployerEvent(long cohortId, DateTime updatedOn, Party assignedBy)
        {
            CohortId = cohortId;
            UpdatedOn = updatedOn;
            AssignedBy = assignedBy;
        }
    }
}