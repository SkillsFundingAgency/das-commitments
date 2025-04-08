namespace SFA.DAS.CommitmentsV2.Shared.ProviderRelationshipsApiClient;

public class HasPermissionRequest
{
    public long Ukprn { get; set; }
    public long AccountLegalEntityId { get; set; }
    public int Operations { get; set; }
}