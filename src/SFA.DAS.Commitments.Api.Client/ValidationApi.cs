using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Newtonsoft.Json;
using SFA.DAS.Authentication.Extensions.Legacy;
using SFA.DAS.Commitments.Api.Client.Configuration;
using SFA.DAS.Commitments.Api.Client.Interfaces;
using SFA.DAS.Commitments.Api.Types.Validation;

namespace SFA.DAS.Commitments.Api.Client
{
    public class ValidationApi : ApiClientBase, IValidationApi
    {
        private readonly ICommitmentsApiClientConfiguration _configuration;

        public ValidationApi(HttpClient client, ICommitmentsApiClientConfiguration configuration)
            : base(client)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            _configuration = configuration;
        }

        public async Task<ApprenticeshipOverlapValidationResult> ValidateOverlapping(ApprenticeshipOverlapValidationRequest request)
        {
            var wrapper = new List<ApprenticeshipOverlapValidationRequest> { request };
            var url = $"{_configuration.BaseUrl}api/validation/apprenticeships/overlapping";
            var wrappedResult = await GetValidation(url, wrapper);
            return wrappedResult.SingleOrDefault();
        }

        public async Task<IEnumerable<ApprenticeshipOverlapValidationResult>> ValidateOverlapping(IEnumerable<ApprenticeshipOverlapValidationRequest> requests)
        {
            var url = $"{_configuration.BaseUrl}api/validation/apprenticeships/overlapping";
            return await GetValidation(url, requests);
        }

        private async Task<IEnumerable<ApprenticeshipOverlapValidationResult>> GetValidation(string url, IEnumerable<ApprenticeshipOverlapValidationRequest> requests)
        {
            var data = JsonConvert.SerializeObject(requests);
            var result = await PostAsync(url, data);
            return JsonConvert.DeserializeObject<IEnumerable<ApprenticeshipOverlapValidationResult>>(result);
        }

        public async Task<IEnumerable<ApprenticeshipEmailOverlapValidationResult>> ValidateEmailOverlapping(IEnumerable<ApprenticeshipEmailOverlapValidationRequest> requests)
        {
            var url = $"{_configuration.BaseUrl}api/validation/apprenticeships/emailoverlapping";
            var data = JsonConvert.SerializeObject(requests);
            var result = await PostAsync(url, data);
            return JsonConvert.DeserializeObject<IEnumerable<ApprenticeshipEmailOverlapValidationResult>>(result);
        }
    }
}