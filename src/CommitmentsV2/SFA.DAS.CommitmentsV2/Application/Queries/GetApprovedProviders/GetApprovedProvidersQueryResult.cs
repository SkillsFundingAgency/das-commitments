namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprovedProviders
{
    public class GetApprovedProvidersQueryResult
    {
        public long[] ProviderIds { get; }

        public GetApprovedProvidersQueryResult(IEnumerable<long> providerIds)
        {
            ProviderIds = providerIds.ToArray();
        }
    }
}
