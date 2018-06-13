using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SFA.DAS.Commitments.Api.Client.Configuration;
using SFA.DAS.Commitments.Api.Client.Interfaces;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Http;

namespace SFA.DAS.Commitments.Api.Client
{
    public class StatisticsApi : ApiClientBase, IStatisticsApi
    {
        private readonly ICommitmentsApiClientConfiguration _configuration;

        public StatisticsApi(HttpClient client, ICommitmentsApiClientConfiguration configuration) : base(client)
        {
            _configuration = configuration;
        }

        public async Task<ConsistencyStatistics> GetStatistics()
        {
            var url = $"{_configuration.BaseUrl}api/statistics";
            var content = await GetAsync(url);
            return JsonConvert.DeserializeObject<ConsistencyStatistics>(content);
        }
    }
}
