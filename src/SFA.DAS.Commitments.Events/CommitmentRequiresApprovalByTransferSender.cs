using SFA.DAS.Messaging.Attributes;

namespace SFA.DAS.Commitments.Events
{
    [MessageGroup("commitment_requires_approval_by_transfer_sender")]
    public class CommitmentRequiresApprovalByTransferSender
    {
        public CommitmentRequiresApprovalByTransferSender()
        {
        }

        public CommitmentRequiresApprovalByTransferSender(long accountId, long providerId, long commitmentId, long transferSenderId)
        {
            AccountId = accountId;
            ProviderId = providerId;
            CommitmentId = commitmentId;
            TransferSenderId = transferSenderId;
        }

        public long AccountId { get; set; }
        public long ProviderId { get; set; }
        public long CommitmentId { get; set; }
        public long TransferSenderId { get; }
    }
}
