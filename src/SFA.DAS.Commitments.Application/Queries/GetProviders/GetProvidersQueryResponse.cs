using System.Collections.Generic;
using SFA.DAS.Commitments.Api.Types;

namespace SFA.DAS.Commitments.Application.Queries.GetProviders
{
    public class GetProvidersQueryResponse
    {
        public List<ProviderResponse> Providers { get ; set ; }
    }
}    