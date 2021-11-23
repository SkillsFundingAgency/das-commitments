using SFA.DAS.CommitmentsV2.Types;
using System;

namespace SFA.DAS.CommitmentsV2.Models
{
    // This is a pseudo-entity to represent the result of the [GetTransferRequestsForSender] and [GetTransferRequestsForReceiver] stored proc, it's not a table in the database.
    public class TransferRequestSummary
    {   
        public long TransferRequestId { get; set; }
        public long ReceivingEmployerAccountId { get; set; }
        public string CohortReference { get; set; }
        public long SendingEmployerAccountId { get; set; }
        public long CommitmentId { get; set; }
        public decimal TransferCost { get; set; }
        public TransferApprovalStatus Status { get; set; }
        public string ApprovedOrRejectedByUserName { get; set; }
        public string ApprovedOrRejectedByUserEmail { get; set; }
        public DateTime? ApprovedOrRejectedOn { get; set; }        
        public DateTime CreatedOn { get; set; }
        public int FundingCap { get; set; }
    }
}
