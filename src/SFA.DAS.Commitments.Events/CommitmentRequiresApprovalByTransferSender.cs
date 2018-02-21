using SFA.DAS.Messaging.Attributes;

namespace SFA.DAS.Commitments.Events
{
    [MessageGroup("commitment_requires_approval_by_transfer_sender")]
    public class CommitmentRequiresApprovalByTransferSender
    {
        public CommitmentRequiresApprovalByTransferSender()
        {
        }

        public CommitmentRequiresApprovalByTransferSender(long accountId, long commitmentId, long transferSenderId, decimal transferCost)
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
