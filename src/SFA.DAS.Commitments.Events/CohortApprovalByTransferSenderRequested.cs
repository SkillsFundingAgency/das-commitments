using SFA.DAS.Messaging.Attributes;

namespace SFA.DAS.Commitments.Events
{
    [MessageGroup("cohort_approval_by_transfer_sender_requested")]
    public class CohortApprovalByTransferSenderRequested
    {
        //Needs a parameterless constructor to work with the message processing
        public CohortApprovalByTransferSenderRequested() { }

        public CohortApprovalByTransferSenderRequested(long transferRequestId, long accountId, long commitmentId, long transferSenderId, decimal transferCost)
        {
            TransferRequestId = transferRequestId;
            ReceivingEmployerAccountId = accountId;
            CommitmentId = commitmentId;
            SendingEmployerAccountId = transferSenderId;
            TransferCost = transferCost;
        }
        public long TransferRequestId { get; set; }
        public long ReceivingEmployerAccountId { get; set; }
        public long CommitmentId { get; set; }
        public long SendingEmployerAccountId { get; set; }
        public decimal TransferCost { get; set; }
    }
}
