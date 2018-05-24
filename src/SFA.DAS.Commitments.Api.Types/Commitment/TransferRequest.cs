using System;
using System.Collections.Generic;

namespace SFA.DAS.Commitments.Api.Types.Commitment
{
    public class TransferRequest
    {
        public long TransferRequestId { get; set; }
        public long ReceivingEmployerAccountId { get; set; }
        public long CommitmentId { get; set; }
        public long SendingEmployerAccountId { get; set; }
        public string TransferSenderName { get; set; }
        public string LegalEntityName { get; set; }
        public decimal TransferCost { get; set; }
        public List<TrainingCourseSummary> TrainingList { get; set; }
        public TransferApprovalStatus Status { get; set; }
        public string ApprovedOrRejectedByUserName { get; set; }
        public string ApprovedOrRejectedByUserEmail { get; set; }
        public DateTime? ApprovedOrRejectedOn { get; set; }
    }
}
