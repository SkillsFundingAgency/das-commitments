using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses
{
    public class GetApprovedProvidersResponse
    {
        public GetApprovedProvidersResponse(IEnumerable<long> providerIds)
        {
            ProviderIds = providerIds.ToArray();
        }
        public long[] ProviderIds { get; }
    }
}
