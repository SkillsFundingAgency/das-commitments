using System;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.Http;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;

namespace SFA.DAS.CommitmentsV2.Api.Client
{
    public class CommitmentsApiClient : ICommitmentsApiClient
    {
        private readonly IRestHttpClient _client;

        public CommitmentsApiClient(IRestHttpClient client)
        {
            _client = client;
        }
        public async Task<bool> HealthCheck()
        {
            var result = await _client.Get("api/health-check");
            if (result?.Equals("Healthy", StringComparison.InvariantCultureIgnoreCase) == true)
            {
                return true;
            }

            return false;
        }

        public Task<GetCohortResponse> GetCohort(long cohortId, CancellationToken cancellationToken = default)
        {
            return _client.PostAsJson<long, GetCohortResponse>("api/cohorts", cohortId, cancellationToken);
        }

        public Task<CreateCohortResponse> CreateCohort(CreateCohortRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PostAsJson<CreateCohortRequest, CreateCohortResponse>("api/cohorts", request, cancellationToken);
        }

        public Task<UpdateDraftApprenticeshipResponse> UpdateDraftApprenticeship(long cohortId, long apprenticeshipId, UpdateDraftApprenticeshipRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PostAsJson<UpdateDraftApprenticeshipRequest, UpdateDraftApprenticeshipResponse>(
                $"api/cohorts/{cohortId}/draft-Apprenticeships/{apprenticeshipId}", request, cancellationToken);
        }

        public Task<AccountLegalEntityResponse> GetLegalEntity(long accountLegalEntityId, CancellationToken cancellationToken = default)
        {
            return _client.Get<AccountLegalEntityResponse>($"api/accountlegalentity/{accountLegalEntityId}", null, cancellationToken);
        }

        public Task<string> SecureCheck()
        {
            return _client.Get("api/test");
        }

        public Task<string> SecureEmployerCheck()
        {
            return _client.Get("api/test/employer");
        }

        public Task<string> SecureProviderCheck()
        {
            return _client.Get("api/test/provider");
        }
    }
}