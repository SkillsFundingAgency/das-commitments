namespace SFA.DAS.Commitments.Domain.Entities
{
    public class Relationship
    {
        public long Id { get; set; }
        public long EmployerAccountId { get; set; }
        public string LegalEntityId { get; set; }
        public string LegalEntityName { get; set; }
        public string LegalEntityAddress { get; set; }
        public SFA.DAS.Common.Domain.Types.OrganisationType LegalEntityOrganisationType { get; set; }
        public long ProviderId { get; set; }
        public string ProviderName { get; set; }
        public bool? Verified { get; set; }
    }
}
