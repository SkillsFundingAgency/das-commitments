using System;
using System.ComponentModel.DataAnnotations;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Api.Types.Commitment.Types;
using OrganisationType = SFA.DAS.Common.Domain.Types.OrganisationType;

namespace SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Entities
{
    public class DbSetupCommitment
    {
        public long Id { get; set; }
        [StringLength(100)]
        public string Reference { get; set; }
        public long EmployerAccountId { get; set; }
        [StringLength(50)]
        public string LegalEntityId { get; set; }
        [StringLength(100)]
        public string LegalEntityName { get; set; }
        [StringLength(256)]
        public string LegalEntityAddress { get; set; }
        public OrganisationType LegalEntityOrganisationType { get; set; }
        public long? ProviderId { get; set; }
        [StringLength(100)]
        public string ProviderName { get; set; }
        public CommitmentStatus CommitmentStatus { get; set; }
        public EditStatus EditStatus { get; set; }
        public DateTime? CreatedOn { get; set; }
        public short LastAction { get; set; }
        [StringLength(255)]
        public string LastUpdatedByEmployerName { get; set; }
        [StringLength(255)]
        public string LastUpdatedByEmployerEmail { get; set; }
        [StringLength(255)]
        public string LastUpdatedByProviderName { get; set; }
        [StringLength(255)]
        public string LastUpdatedByProviderEmail { get; set; }
        public long? TransferSenderId { get; set; }
        [StringLength(100)]
        public string TransferSenderName { get; set; }
        public TransferApprovalStatus? TransferApprovalStatus { get; set; }
        [StringLength(255)]
        public string TransferApprovalActionedByEmployerName { get; set; }
        [StringLength(255)]
        public string TransferApprovalActionedByEmployerEmail { get; set; }
        public DateTime? TransferApprovalActionedOn { get; set; }
    }
}
