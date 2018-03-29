using System.Collections.Generic;

namespace SFA.DAS.Commitments.Domain.Entities
{
    public class CommitmentSummary
    {
        public CommitmentSummary()
        {
            Messages = new List<Message>();
        }

        public long Id { get; set; }
        public string Reference { get; set; }
        public long EmployerAccountId { get; set; }
        public string LegalEntityId { get; set; }
        public string LegalEntityName { get; set; }
        public long? ProviderId { get; set; }
        public string ProviderName { get; set; }
        public CommitmentStatus CommitmentStatus { get; set; }
        public EditStatus EditStatus { get; set; }
        public int ApprenticeshipCount { get; set; }
        public AgreementStatus AgreementStatus { get; set; }
        public LastAction LastAction { get; set; }
        public long? TransferSenderId { get; set; }
        public TransferApprovalStatus TransferApprovalStatus { get; set; }
        public bool EmployerCanApproveCommitment { get; set; }
        public bool ProviderCanApproveCommitment { get; set; }
        public string LastUpdatedByEmployerName { get; set; }
        public string LastUpdatedByEmployerEmail { get; set; }
        public string LastUpdatedByProviderName { get; set; }
        public string LastUpdatedByProviderEmail { get; set; }
        public List<Message> Messages { get; set; }
    }
}
