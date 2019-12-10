using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses
{
    public class GetApprovedProvidersResponse
    {
        public long[] ProviderIds { get; }
        public long AccountId { get; set; }

        public GetApprovedProvidersResponse(long accountId, IEnumerable<long> providerIds)
        {
            AccountId = accountId;
            ProviderIds = providerIds.ToArray();
        }
    }
}
