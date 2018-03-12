using System;

namespace SFA.DAS.Commitments.Api.Types.Commitment
{
    public class TransferSenderInfo
    {
        public long? TransferSenderId { get; set; }
        public string TransferSenderName { get; set; }
        public TransferApprovalStatus TransferApprovalStatus { get; set; }
        public string TransferApprovalSetBy { get; set; }
        public DateTime? TransferApprovalSetOn { get; set; }
    }
}