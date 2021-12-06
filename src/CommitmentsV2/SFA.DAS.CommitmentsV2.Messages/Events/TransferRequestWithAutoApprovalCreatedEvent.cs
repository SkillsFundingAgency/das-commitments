using System;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class TransferRequestWithAutoApprovalCreatedEvent
    {
        public long TransferRequestId { get; }
        public int PledgeApplicationId { get; }
        public DateTime CreatedOn { get; }

        public TransferRequestWithAutoApprovalCreatedEvent(long transferRequestId, int pledgeApplicationId, DateTime createdOn)
        {
            TransferRequestId = transferRequestId;
            PledgeApplicationId = pledgeApplicationId;
            CreatedOn = createdOn;
        }
    }
}
