namespace SFA.DAS.CommitmentsV2.Shared.ProviderRelationshipsApiClient;

public class GetAccountProviderLegalEntitiesWithPermissionRequest
{
    public string AccountHashedId { get; set; }
    public long Ukprn { get; set; }
    public int Operations { get; set; }
}