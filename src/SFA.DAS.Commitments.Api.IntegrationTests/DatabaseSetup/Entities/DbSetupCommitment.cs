using System;
using System.ComponentModel.DataAnnotations;
using SFA.DAS.Commitments.Api.Types.Commitment.Types;
using SFA.DAS.Common.Domain.Types;

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
    }
}
