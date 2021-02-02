using System.Collections.Generic;

namespace SFA.DAS.Commitments.Api.Types
{
    public class GetProvidersResponse
    {
        public IEnumerable<ProviderResponse> Providers { get; set; }
    }
}