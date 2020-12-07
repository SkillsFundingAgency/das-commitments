using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Polly;
using SFA.DAS.Commitments.Domain.Api.Requests;
using SFA.DAS.Commitments.Domain.Api.Types;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Infrastructure.Api.Requests;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.Commitments.Infrastructure.Services
{
    public class AssessmentOrgsService : IAssessmentOrgs
    {
        private readonly ILog _logger;
        private readonly IApiClient _apiClient;
        private readonly Policy _retryPolicy;

        public AssessmentOrgsService(IApiClient apiClient, ILog logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));

            _retryPolicy = Policy
                .Handle<Exception>()
                .RetryAsync(3,
                    (exception, retryCount) =>
                    {
                        _logger.Warn($"Error connecting to Assessment Orgs Api: ({exception.Message}). Retrying...attempt {retryCount})");
                    }
                );
        }

        public async Task<IEnumerable<OrganisationSummary>> All()
        {
            var response = await _retryPolicy.ExecuteAsync(() => _apiClient.Get<EpaoResponse>(new GetEpaoOrganisationsRequest()));
            
            return response.Epaos;
        }

    }
}