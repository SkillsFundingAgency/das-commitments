using SFA.DAS.CommitmentsV2.Types;
using System;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class CohortTransferApprovalRequestedEvent
    {
        public long CohortId { get; }
        public DateTime UpdatedOn { get; }
        public Party LastApprovedByParty { get; set; }

        public CohortTransferApprovalRequestedEvent(long cohortId, DateTime updatedOn, Party lastApprovedByParty)
        {
            LastApprovedByParty = lastApprovedByParty;
            CohortId = cohortId;
            UpdatedOn = updatedOn;
        }
    }
}