using System;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class TransferRequestCreatedEvent
    {
        public long TransferRequestId { get; }
        public long CohortId { get; }
        public DateTime CreatedOn { get; }

        public TransferRequestCreatedEvent(long transferRequestId, long cohortId, DateTime createdOn)
        {
            TransferRequestId = transferRequestId;
            CohortId = cohortId;
            CreatedOn = createdOn;
        }
    }
}