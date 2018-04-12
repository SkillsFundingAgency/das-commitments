using System;

namespace SFA.DAS.Commitments.Api.Types.Commitment
{
    public class TransferRequestSummary
    {
        public string HashedTransferRequestId { get; set; }
        public string HashedReceivingEmployerAccountId { get; set; }
        public string HashedCohortRef { get; set; }
        public string HashedSendingEmployerAccountId { get; set; }
        public decimal TransferCost { get; set; }
        public TransferApprovalStatus Status { get; set; }
        public string ApprovedOrRejectedByUserName { get; set; }
        public string ApprovedOrRejectedByUserEmail { get; set; }
        public DateTime? ApprovedOrRejectedOn { get; set; }
        public TransferType TransferType { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
