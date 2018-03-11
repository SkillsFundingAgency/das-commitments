using SFA.DAS.Messaging.Attributes;

namespace SFA.DAS.Commitments.Events
{
    [MessageGroup("cohort_approval_by_transfer_sender_requested")]
    public class CohortApprovalByTransferSenderRequested
    {
        public CohortApprovalByTransferSenderRequested()
        {
        }

        public CohortApprovalByTransferSenderRequested(long accountId, long commitmentId, long transferSenderId, decimal transferCost)
        {
            ReceivingEmployerAccountId = accountId;
            CommitmentId = commitmentId;
            SendingEmployerAccountId = transferSenderId;
            TransferCost = transferCost;
        }

        public long ReceivingEmployerAccountId { get; set; }
        public long CommitmentId { get; set; }
        public long SendingEmployerAccountId { get; set; }
        public decimal TransferCost { get; set; }
    }
}
