using System.Collections.Generic;
using SFA.DAS.Commitments.Api.Types.Commitment.Types;

namespace SFA.DAS.Commitments.Api.Types.Commitment
{
    public sealed class CommitmentListItem
    {
        public CommitmentListItem()
        {
            Messages = new List<MessageView>();
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
        public bool CanBeApproved { get; set; }
        public LastUpdateInfo EmployerLastUpdateInfo { get; set; }
        public LastUpdateInfo ProviderLastUpdateInfo { get; set; }
        public List<MessageView> Messages { get; set; }
    }
}
