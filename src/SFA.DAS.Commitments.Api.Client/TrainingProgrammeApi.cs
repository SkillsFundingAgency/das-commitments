using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SFA.DAS.Authentication.Extensions.Legacy;
using SFA.DAS.Commitments.Api.Client.Configuration;
using SFA.DAS.Commitments.Api.Client.Interfaces;
using SFA.DAS.Commitments.Api.Types.TrainingProgramme;

namespace SFA.DAS.Commitments.Api.Client
{
    public class TrainingProgrammeApi : ApiClientBase, ITrainingProgrammeApi
    {
        private readonly ICommitmentsApiClientConfiguration _configuration;
        public TrainingProgrammeApi (HttpClient client, ICommitmentsApiClientConfiguration configuration)
            : base(client)
        {
            if(configuration == null)
                throw new ArgumentException(nameof(configuration));
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            _configuration = configuration;
        }

        public async Task<GetAllTrainingProgrammeStandardsResponse> GetAllStandards()
        {
            var url = $"{_configuration.BaseUrl}api/trainingprogramme/standards";
            var content = await GetAsync(url);
            
            return JsonConvert.DeserializeObject<GetAllTrainingProgrammeStandardsResponse>(content);
        }
        
        public async Task<GetAllTrainingProgrammesResponse> GetAll()
        {
            var url = $"{_configuration.BaseUrl}api/trainingprogramme/all";
            var content = await GetAsync(url);
            
            return JsonConvert.DeserializeObject<GetAllTrainingProgrammesResponse>(content);
        }
        
        public async Task<GetTrainingProgrammeResponse> Get(string id)
        {
            var url = $"{_configuration.BaseUrl}api/trainingprogramme/{id}";
            var content = await GetAsync(url);
            
            return JsonConvert.DeserializeObject<GetTrainingProgrammeResponse>(content);
        }
    }
}