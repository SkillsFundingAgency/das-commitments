using System;

namespace SFA.DAS.Commitments.Api.Types.Commitment
{
    public class TransferSender
    {
        public long? Id { get; set; }
        public string Name { get; set; }
        public TransferApprovalStatus? TransferApprovalStatus { get; set; }
        public DateTime? TransferApprovalSetOn { get; set; }
    }
}