using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polly;
using SFA.DAS.Apprenticeships.Api.Types.AssessmentOrgs;
using SFA.DAS.AssessmentOrgs.Api.Client;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.Commitments.Infrastructure.Services
{
    public class AssessmentOrgsService : IAssessmentOrgs
    {
        private readonly ILog _logger;
        private readonly IAssessmentOrgsApiClient _assessmentOrgsApi;
        private readonly Policy _retryPolicy;

        public AssessmentOrgsService(IAssessmentOrgsApiClient assessmentOrgsApi, ILog logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _assessmentOrgsApi = assessmentOrgsApi ?? throw new ArgumentNullException(nameof(assessmentOrgsApi));

            _retryPolicy = Policy
                .Handle<Exception>()
                .RetryAsync(3,
                    (exception, retryCount) =>
                    {
                        _logger.Warn($"Error connecting to Assessment Orgs Api: ({exception.Message}). Retrying...attempt {retryCount})");
                    }
                );
        }

        public async Task<IEnumerable<OrganisationSummary>> AllAsync()
        {
            return await _retryPolicy.ExecuteAsync(() => _assessmentOrgsApi.FindAllAsync());
        }
    }
}