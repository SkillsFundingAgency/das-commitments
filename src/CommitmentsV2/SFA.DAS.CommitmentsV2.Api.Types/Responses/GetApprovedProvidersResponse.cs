using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses;

public class GetApprovedProvidersResponse
{
    public long[] ProviderIds { get; }

    public GetApprovedProvidersResponse(IEnumerable<long> providerIds)
    {
        ProviderIds = providerIds.ToArray();
    }
}