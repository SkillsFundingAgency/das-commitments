using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

using Newtonsoft.Json;

using SFA.DAS.Commitments.Api.Client.Configuration;
using SFA.DAS.Commitments.Api.Client.Interfaces;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Http;

namespace SFA.DAS.Commitments.Api.Client
{
    public class ApprenticeshipApi : ApiClientBase, IApprenticeshipApi
    {
        private readonly ICommitmentsApiClientConfiguration _configuration;

        public ApprenticeshipApi(HttpClient httpClient, ICommitmentsApiClientConfiguration configuration)
            : base(httpClient)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            _configuration = configuration;
        }

        [Obsolete("Moved to provider / employer API")]
        public async Task<IEnumerable<PriceHistory>> GetPriceHistory(long apprenticeshipId)
        {
            var url = $"{_configuration.BaseUrl}api/apprenticeships/{apprenticeshipId}/prices";
            var content = await GetAsync(url);
            return JsonConvert.DeserializeObject<IEnumerable<PriceHistory>>(content);
        }
    }
}