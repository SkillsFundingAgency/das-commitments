using SFA.DAS.CommitmentsV2.Types;
using System;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class TransferRequestCreatedEvent
    {
        public long TransferRequestId { get; }
        public long CohortId { get; }
        public DateTime CreatedOn { get; }

        public Party LastApprovedByParty { get; set; }

        public TransferRequestCreatedEvent(long transferRequestId, long cohortId, DateTime createdOn, Party lastApprovedByParty)
        {
            TransferRequestId = transferRequestId;
            CohortId = cohortId;
            CreatedOn = createdOn;
            LastApprovedByParty = lastApprovedByParty;
        }
    }
}