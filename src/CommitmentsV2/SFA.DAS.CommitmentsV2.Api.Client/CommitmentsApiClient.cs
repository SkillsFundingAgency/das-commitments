using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.Http;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Client
{
    public class CommitmentsApiClient : ICommitmentsApiClient
    {
        private readonly IRestHttpClient _client;

        public CommitmentsApiClient(IRestHttpClient client)
        {
            _client = client;
        }
        public Task Ping()
        {
            return _client.Get("api/ping");
        }

        public Task<WhoAmIResponse> WhoAmI()
        {
            return _client.Get<WhoAmIResponse>("api/whoami");
        }

        public Task AddDraftApprenticeship(long cohortId, AddDraftApprenticeshipRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PostAsJson($"api/cohorts/{cohortId}/draft-apprenticeships", request, cancellationToken);
        }

        public Task ApproveCohort(long cohortId, ApproveCohortRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PostAsJson($"api/cohorts/{cohortId}/approve", request, cancellationToken);
        }

        public Task<CreateCohortResponse> CreateCohort(CreateCohortRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PostAsJson<CreateCohortRequest, CreateCohortResponse>("api/cohorts", request, cancellationToken);
        }

        public Task<CreateCohortResponse> CreateCohort(CreateCohortWithOtherPartyRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PostAsJson<CreateCohortWithOtherPartyRequest, CreateCohortResponse>("api/cohorts/create-with-other-party", request, cancellationToken);
        }

        public Task<GetApprenticeshipsResponse> GetApprenticeships(GetApprenticeshipRequest request, CancellationToken cancellationToken = default)
        {
            var pageQuery = CreatePageQuery(request);
            var sortField = CreateSortFieldQuery(request);
            var filterQuery = CreateFilterQuery(request);

            return _client.Get<GetApprenticeshipsResponse>(
                $"api/apprenticeships/?providerId={request.ProviderId}&reverseSort={request.ReverseSort}{sortField}{filterQuery}{pageQuery}", null, cancellationToken);
        }

        public Task<GetApprenticeshipsFilterValuesResponse> GetApprenticeshipsFilterValues(long providerId, CancellationToken cancellationToken = default)
        {
            return _client.Get<GetApprenticeshipsFilterValuesResponse>(
                $"api/apprenticeships/filters?providerId={providerId}", null, cancellationToken);
        }

        public Task<GetCohortResponse> GetCohort(long cohortId, CancellationToken cancellationToken = default)
        {
            return _client.Get<GetCohortResponse>($"api/cohorts/{cohortId}", null, cancellationToken);
        }

        public Task<GetDraftApprenticeshipResponse> GetDraftApprenticeship(long cohortId, long apprenticeshipId,
            CancellationToken cancellationToken = default)
        {
            return _client.Get<GetDraftApprenticeshipResponse>($"api/cohorts/{cohortId}/draft-apprenticeships/{apprenticeshipId}", null, cancellationToken);
        }
        public Task<GetDraftApprenticeshipsResponse> GetDraftApprenticeships(long cohortId, CancellationToken cancellationToken = default)
        {
            return _client.Get<GetDraftApprenticeshipsResponse> ($"api/cohorts/{cohortId}/draft-apprenticeships", null, cancellationToken);
        }

        public Task<AccountLegalEntityResponse> GetLegalEntity(long accountLegalEntityId, CancellationToken cancellationToken = default)
        {
            return _client.Get<AccountLegalEntityResponse>($"api/accountlegalentity/{accountLegalEntityId}", null, cancellationToken);
        }

        public Task<GetProviderResponse> GetProvider(long providerId, CancellationToken cancellationToken = default)
        {
            return _client.Get<GetProviderResponse>($"api/providers/{providerId}", null, cancellationToken);
        }

        public Task SendCohort(long cohortId, SendCohortRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PostAsJson($"api/cohorts/{cohortId}/send", request, cancellationToken);
        }

        public Task UpdateDraftApprenticeship(long cohortId, long apprenticeshipId, UpdateDraftApprenticeshipRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PutAsJson<UpdateDraftApprenticeshipRequest>(
                $"api/cohorts/{cohortId}/draft-apprenticeships/{apprenticeshipId}", request, cancellationToken);
        }

        public Task DeleteDraftApprenticeship(long cohortId, long apprenticeshipId, DeleteDraftApprenticeshipRequest request,
            CancellationToken cancellationToken = default)
        {
            return _client.PostAsJson<DeleteDraftApprenticeshipRequest>(
                $"api/cohorts/{cohortId}/draft-apprenticeships/{apprenticeshipId}", request, cancellationToken);
        }

        public Task<bool> IsAgreementSigned(AgreementSignedRequest request, CancellationToken cancellationToken)
        {
            string queryString = null;

            if (request.AgreementFeatures?.Length > 0)
            {
                foreach (var agreementFeature in request.AgreementFeatures)
                {
                    if (queryString == null)
                    {
                        queryString = $"?agreementFeatures={agreementFeature}";
                    }
                    else
                    {
                        queryString += $"&agreementFeatures={agreementFeature}";
                    }
                }
            }

            return _client.Get<bool>($"api/employer-agreements/{request.AccountLegalEntityId}/signed{queryString}", null, 
                cancellationToken);
        }

        public Task<long?> GetLatestAgreementId(long accountLegalEntityId, CancellationToken cancellationToken)
        {
            return _client.Get<long?>($"api/employer-agreements/{accountLegalEntityId}/latest-id", null, cancellationToken);
        }

        public Task DeleteCohort(long cohortId, UserInfo userInfo, CancellationToken cancellationToken)
        {
            return _client.PostAsJson($"api/cohorts/{cohortId}/delete", userInfo, cancellationToken);
        }

        public Task<AccountResponse> GetAccount(long accountId, CancellationToken cancellationToken = default)
        {
            return _client.Get<AccountResponse>($"api/accounts/{accountId}", null, cancellationToken);
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

        private static string CreateFilterQuery(GetApprenticeshipRequest request)
        {
            var filterQuery = string.Empty;

            if (!string.IsNullOrEmpty(request.EmployerName))
            {
                filterQuery += $"&employerName={WebUtility.UrlEncode(request.EmployerName)}";
            }

            if (!string.IsNullOrEmpty(request.CourseName))
            {
                filterQuery += $"&courseName={WebUtility.UrlEncode(request.CourseName)}";
            }

            if (!string.IsNullOrEmpty(request.Status))
            {
                filterQuery += $"&status={WebUtility.UrlEncode(request.Status)}";
            }

            if (request.StartDate.HasValue)
            {
                filterQuery += $"&startDate={WebUtility.UrlEncode(request.StartDate.Value.ToShortDateString())}";
            }

            if (request.EndDate.HasValue)
            {
                filterQuery += $"&endDate={WebUtility.UrlEncode(request.EndDate.Value.ToShortDateString())}";
            }

            return filterQuery;
        }

        private static string CreateSortFieldQuery(GetApprenticeshipRequest request)
        {
            var sortField = "";

            if (!string.IsNullOrEmpty(request.SortField))
            {
                sortField = $"&sortField={request.SortField}";
            }

            return sortField;
        }

        private static string CreatePageQuery(GetApprenticeshipRequest request)
        {
            var pageQuery = string.Empty;

            if (request.PageNumber > 0)
            {
                pageQuery += $"pageNumber={request.PageNumber}";
            }

            if (request.PageItemCount > 0)
            {
                pageQuery += $"{(!string.IsNullOrEmpty(pageQuery) ? "&" : "")}pageItemCount={request.PageItemCount}";
            }

            if (!string.IsNullOrEmpty(pageQuery))
            {
                pageQuery = $"&{pageQuery}";
            }

            return pageQuery;
        }
    }
}