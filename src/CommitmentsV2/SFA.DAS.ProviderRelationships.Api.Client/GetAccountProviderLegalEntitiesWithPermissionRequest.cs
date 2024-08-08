namespace SFA.DAS.ProviderRelationships.Api.Client
{
    public class GetAccountProviderLegalEntitiesWithPermissionRequest
    {
        public string AccountHashedId { get; set; }
        public long Ukprn { get; set; }
        public List<Operation> Operations { get; set; } = new List<Operation>();
    }
}
