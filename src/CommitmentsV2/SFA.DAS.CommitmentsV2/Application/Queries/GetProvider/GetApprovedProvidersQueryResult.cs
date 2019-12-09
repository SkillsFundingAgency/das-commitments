using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetProvider
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
