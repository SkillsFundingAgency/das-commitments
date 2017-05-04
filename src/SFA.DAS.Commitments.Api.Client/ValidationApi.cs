using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SFA.DAS.Commitments.Api.Client.Configuration;
using SFA.DAS.Commitments.Api.Client.Interfaces;
using SFA.DAS.Commitments.Api.Types.Validation;

namespace SFA.DAS.Commitments.Api.Client
{
    public class ValidationApi : IValidationApi
    {
        private readonly ICommitmentsApiClientConfiguration _configuration;
        private readonly SecureHttpClient _client;
        private string BaseUrl => _configuration.BaseUrl.EndsWith("/") ? _configuration.BaseUrl : _configuration.BaseUrl + "/";

        public ValidationApi(ICommitmentsApiClientConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            _configuration = configuration;
        }

        internal ValidationApi(ICommitmentsApiClientConfiguration configuration, SecureHttpClient client)
        {
            _configuration = configuration;
            _client = client;
        }

        public async Task<ApprenticeshipOverlapValidationResult> ValidateOverlapping(ApprenticeshipOverlapValidationRequest request)
        {
            var wrapper = new List<ApprenticeshipOverlapValidationRequest> { request };
            var url = $"{BaseUrl}api/validation/apprenticeships/overlapping";
            var wrappedResult = await GetValidation(url, wrapper);
            return wrappedResult.SingleOrDefault();
        }

        public async Task<IEnumerable<ApprenticeshipOverlapValidationResult>> ValidateOverlapping(IEnumerable<ApprenticeshipOverlapValidationRequest> requests)
        {
            var url = $"{BaseUrl}api/validation/apprenticeships/overlapping";
            return await GetValidation(url, requests);
        }

        private async Task<IEnumerable<ApprenticeshipOverlapValidationResult>> GetValidation(string url, IEnumerable<ApprenticeshipOverlapValidationRequest> requests)
        {
            var data = JsonConvert.SerializeObject(requests);
            var result = await _client.PostAsync(url, data);
            return JsonConvert.DeserializeObject<IEnumerable<ApprenticeshipOverlapValidationResult>>(result);
        }
    }
}