namespace SFA.DAS.ProviderRelationships.Api.Client
{
    public class HasPermissionRequest
    {
        public long Ukprn { get; set; }
        public long AccountLegalEntityId { get; set; }
        public Operation Operation { get; set; }
    }
}
