namespace SFA.DAS.ProviderRelationships.Api.Client
{
    public class GetAccountProviderLegalEntitiesWithPermissionRequest
    {
        public string AccountHashedId { get; set; }
        public long Ukprn { get; set; }
        public int Operations { get; set; }
    }
}
