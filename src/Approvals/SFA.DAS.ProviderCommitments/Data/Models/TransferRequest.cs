using System;

namespace SFA.DAS.ProviderCommitments.Data.Models
{
    public partial class TransferRequest
    {
        public long Id { get; set; }
        public long CommitmentId { get; set; }
        public string TrainingCourses { get; set; }
        public decimal Cost { get; set; }
        public byte Status { get; set; }
        public string TransferApprovalActionedByEmployerName { get; set; }
        public string TransferApprovalActionedByEmployerEmail { get; set; }
        public DateTime? TransferApprovalActionedOn { get; set; }
        public DateTime CreatedOn { get; set; }
        public decimal? FundingCap { get; set; }

        public virtual Commitment Commitment { get; set; }
    }
}
