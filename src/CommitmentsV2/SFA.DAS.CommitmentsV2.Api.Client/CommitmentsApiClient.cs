using System;
using System.Collections.Generic;
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
        public Task<GetApprenticeshipsResponse> GetApprenticeships(GetApprenticeshipsRequest request, CancellationToken cancellationToken = default)
        {
            if (request.ProviderId.HasValue && request.AccountId.HasValue)
            {
                throw new NotSupportedException("Api currently does not support both a provider Id and employer account Id lookup for apprentices.");
            }

            var pageQuery = CreatePageQuery(request);
            var sortField = CreateSortFieldQuery(request);
            var filterQuery = CreateFilterQuery(request);

            if (request.ProviderId.HasValue)
            {
                return _client.Get<GetApprenticeshipsResponse>(
                    $"api/apprenticeships/?providerId={request.ProviderId}&reverseSort={request.ReverseSort}{sortField}{filterQuery}{pageQuery}",
                    null, cancellationToken);
            }

            return _client.Get<GetApprenticeshipsResponse>(
                $"api/apprenticeships/?accountId={request.AccountId}&reverseSort={request.ReverseSort}{sortField}{filterQuery}{pageQuery}",
                null, cancellationToken);
        }

        public Task<GetApprenticeshipsFilterValuesResponse> GetApprenticeshipsFilterValues(GetApprenticeshipFiltersRequest request, CancellationToken cancellationToken = default)
        {
            if (request.ProviderId.HasValue && request.EmployerAccountId.HasValue)
            {
                throw new NotSupportedException("Api currently does not support both a provider Id and employer account Id lookup for filters.");
            }

            if (request.ProviderId.HasValue)
            {
                return _client.Get<GetApprenticeshipsFilterValuesResponse>(
                    $"api/apprenticeships/filters?providerId={request.ProviderId}", null, cancellationToken);
            }

            return _client.Get<GetApprenticeshipsFilterValuesResponse>(
                $"api/apprenticeships/filters?employerAccountId={request.EmployerAccountId}", null, cancellationToken);
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
            return _client.Get<GetDraftApprenticeshipsResponse>($"api/cohorts/{cohortId}/draft-apprenticeships", null, cancellationToken);
        }

        public Task<AccountLegalEntityResponse> GetAccountLegalEntity(long accountLegalEntityId, CancellationToken cancellationToken = default)
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

        public Task<GetCohortsResponse> GetCohorts(GetCohortsRequest request, CancellationToken cancellationToken = default)
        {
            return _client.Get<GetCohortsResponse>($"api/cohorts", request, cancellationToken);
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

        public Task StopApprenticeship(long apprenticeshipId, StopApprenticeshipRequest request, CancellationToken cancellationToken)
        {
            return _client.PostAsJson($"api/apprenticeships/{apprenticeshipId}/stop", request, cancellationToken);
        }

        private static string CreateFilterQuery(GetApprenticeshipsRequest request)
        {
            var filterQuery = string.Empty;

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                filterQuery += $"&searchTerm={WebUtility.UrlEncode(request.SearchTerm)}";
            }

            if (!string.IsNullOrEmpty(request.EmployerName))
            {
                filterQuery += $"&employerName={WebUtility.UrlEncode(request.EmployerName)}";
            }

            if (!string.IsNullOrEmpty(request.ProviderName))
            {
                filterQuery += $"&providerName={WebUtility.UrlEncode(request.ProviderName)}";
            }

            if (!string.IsNullOrEmpty(request.CourseName))
            {
                filterQuery += $"&courseName={WebUtility.UrlEncode(request.CourseName)}";
            }

            if (request.Status.HasValue)
            {
                filterQuery += $"&status={WebUtility.UrlEncode(request.Status.Value.ToString())}";
            }

            if (request.StartDate.HasValue)
            {
                filterQuery += $"&startDate={WebUtility.UrlEncode(request.StartDate.Value.ToString("u"))}";
            }

            if (request.EndDate.HasValue)
            {
                filterQuery += $"&endDate={WebUtility.UrlEncode(request.EndDate.Value.ToString("u"))}";
            }

            if (request.AccountLegalEntityId.HasValue)
            {
                filterQuery += $"&accountLegalEntityId={request.AccountLegalEntityId.Value}";
            }

            if (request.StartDateRangeFrom.HasValue)
            {
                filterQuery += $"&startDateRangeFrom={WebUtility.UrlEncode(request.StartDateRangeFrom.Value.ToString("u"))}";
            }

            if (request.StartDateRangeTo.HasValue)
            {
                filterQuery += $"&startDateRangeTo={WebUtility.UrlEncode(request.StartDateRangeTo.Value.ToString("u"))}";
            }

            if (request.Alert.HasValue)
            {
                filterQuery += $"&alert={WebUtility.UrlEncode(request.Alert.Value.ToString())}";
            }

            return filterQuery;
        }

        private static string CreateSortFieldQuery(GetApprenticeshipsRequest request)
        {
            var sortField = "";

            if (!string.IsNullOrEmpty(request.SortField))
            {
                sortField = $"&sortField={request.SortField}";
            }

            return sortField;
        }

        private static string CreatePageQuery(GetApprenticeshipsRequest request)
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
        public Task<GetApprovedProvidersResponse> GetApprovedProviders(long accountId, CancellationToken cancellationToken)
        {
            return _client.Get<GetApprovedProvidersResponse>($"api/accounts/{accountId}/providers/approved", null, cancellationToken);
        }

        public Task<GetProviderPaymentsPriorityResponse> GetProviderPaymentsPriority(long accountId, CancellationToken cancellationToken = default)
        {
            return _client.Get<GetProviderPaymentsPriorityResponse>($"api/accounts/{accountId}/provider-payments-priority", null, cancellationToken);
        }

        public Task UpdateProviderPaymentsPriority(long accountId, UpdateProviderPaymentsPriorityRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PostAsJson($"api/accounts/{accountId}/update-provider-payments-priority", request, cancellationToken);
        }

        public Task<CreateCohortResponse> CreateCohort(CreateEmptyCohortRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PostAsJson<CreateEmptyCohortRequest, CreateCohortResponse>("api/cohorts/create-empty-cohort", request, cancellationToken);
        }

        public Task<GetAllProvidersResponse> GetAllProviders(CancellationToken cancellationToken = default)
        {
            return _client.Get<GetAllProvidersResponse>($"api/providers");
        }

        public Task<GetApprenticeshipResponse> GetApprenticeship(long apprenticeshipId, CancellationToken cancellationToken = default)
        {
            return _client.Get<GetApprenticeshipResponse>($"api/apprenticeships/{apprenticeshipId}", null, cancellationToken);
        }

        public Task<GetPriceEpisodesResponse> GetPriceEpisodes(long apprenticeshipId, CancellationToken cancellationToken = default)
        {
            return _client.Get<GetPriceEpisodesResponse>($"api/apprenticeships/{apprenticeshipId}/price-episodes", null, cancellationToken);
        }

        public Task<GetApprenticeshipUpdatesResponse> GetApprenticeshipUpdates(long apprenticeshipId, GetApprenticeshipUpdatesRequest request, CancellationToken cancellationToken = default)
        {
            var statusQueryParameter = string.Empty;
            if (request.Status.HasValue)
            {
                statusQueryParameter = $"?status={request.Status}";
            }
            return _client.Get<GetApprenticeshipUpdatesResponse>($"api/apprenticeships/{apprenticeshipId}/updates{statusQueryParameter}", null, cancellationToken);
        }

        public Task<GetDataLocksResponse> GetApprenticeshipDatalocksStatus(long apprenticeshipId, CancellationToken cancellationToken = default)
        {
            return _client.Get<GetDataLocksResponse>($"api/apprenticeships/{apprenticeshipId}/datalocks", null, cancellationToken);
        }

        public Task<GetDataLockSummariesResponse> GetApprenticeshipDatalockSummariesStatus(long apprenticeshipId, CancellationToken cancellationToken = default)
        {
            return _client.Get<GetDataLockSummariesResponse>($"api/apprenticeships/{apprenticeshipId}/datalocksummaries", null, cancellationToken);
        }

        public Task AcceptDataLockChanges(long apprenticeshipId, AcceptDataLocksRequestChangesRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PostAsJson($"api/apprenticeships/{apprenticeshipId}/datalocks/accept-changes", request, cancellationToken);
        }
        
        public Task RejectDataLockChanges(long apprenticeshipId, RejectDataLocksRequestChangesRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PostAsJson($"api/apprenticeships/{apprenticeshipId}/datalocks/reject-changes", request, cancellationToken);
        }

        public Task CreateChangeOfPartyRequest(long apprenticeshipId, CreateChangeOfPartyRequestRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PostAsJson($"api/apprenticeships/{apprenticeshipId}/change-of-party-requests", request, cancellationToken);
        }

        public Task<GetChangeOfPartyRequestsResponse> GetChangeOfPartyRequests(long apprenticeshipId, CancellationToken cancellationToken = default)
        {
            return _client.Get<GetChangeOfPartyRequestsResponse>($"api/apprenticeships/{apprenticeshipId}/change-of-party-requests", null, cancellationToken);
        }

        public Task<GetChangeOfProviderChainResponse> GetChangeOfProviderChain(long apprenticeshipId, CancellationToken cancellationToken = default)
        {
            return _client.Get<GetChangeOfProviderChainResponse>($"api/apprenticeships/{apprenticeshipId}/change-of-provider-chain", null, cancellationToken);
        }

        public Task<GetChangeOfEmployerChainResponse> GetChangeOfEmployerChain(long apprenticeshipId, CancellationToken cancellationToken = default)
        {
            return _client.Get<GetChangeOfEmployerChainResponse>($"api/apprenticeships/{apprenticeshipId}/change-of-employer-chain", null, cancellationToken);
        }

        public Task UpdateEndDateOfCompletedRecord(EditEndDateRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PostAsJson($"api/apprenticeships/details/editenddate", request, cancellationToken);
        }

        public Task PauseApprenticeship(PauseApprenticeshipRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PostAsJson($"api/apprenticeships/details/pause", request, cancellationToken);
        }

        public Task ResumeApprenticeship(ResumeApprenticeshipRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PostAsJson($"api/apprenticeships/details/resume", request, cancellationToken);
        }

        public Task<GetAllTrainingProgrammesResponse> GetAllTrainingProgrammes(CancellationToken cancellationToken = default)
        {
            return _client.Get<GetAllTrainingProgrammesResponse>($"api/TrainingProgramme/all", null, cancellationToken);
        }
        public Task<GetAllTrainingProgrammeStandardsResponse> GetAllTrainingProgrammeStandards(CancellationToken cancellationToken = default)
        {
            return _client.Get<GetAllTrainingProgrammeStandardsResponse>($"api/TrainingProgramme/standards", null, cancellationToken);
        }

        public Task<GetTrainingProgrammeResponse> GetTrainingProgramme(string id, CancellationToken cancellationToken = default)
        {
            return _client.Get<GetTrainingProgrammeResponse>($"api/TrainingProgramme/{id}", null, cancellationToken);
        }

        public Task UpdateApprenticeshipStopDate(long apprenticeshipId, ApprenticeshipStopDateRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PutAsJson($"api/apprenticeships/{apprenticeshipId}/stopdate", request, cancellationToken);
		}
		     
        public Task ValidateApprenticeshipForEdit(ValidateApprenticeshipForEditRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PostAsJson($"api/apprenticeships/edit/validate", request, cancellationToken);
        }

        public Task AcceptApprenticeshipUpdates(long apprenticeshipId, AcceptApprenticeshipUpdatesRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PostAsJson($"api/apprenticeships/{apprenticeshipId}/updates/accept-apprenticeship-update", request, cancellationToken);
        }

        public Task RejectApprenticeshipUpdates(long apprenticeshipId, RejectApprenticeshipUpdatesRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PostAsJson($"api/apprenticeships/{apprenticeshipId}/updates/reject-apprenticeship-update", request, cancellationToken);
        }

        public Task UndoApprenticeshipUpdates(long apprenticeshipId, UndoApprenticeshipUpdatesRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PostAsJson($"api/apprenticeships/{apprenticeshipId}/updates/undo-apprenticeship-update", request, cancellationToken);
        }

        public Task<EditApprenticeshipResponse> EditApprenticeship(EditApprenticeshipApiRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PostAsJson<EditApprenticeshipApiRequest, EditApprenticeshipResponse>($"api/apprenticeships/edit", request, cancellationToken);

        }

        public Task<GetTransferRequestResponse> GetTransferRequestForSender(long transferSenderId, long transferRequestId, CancellationToken cancellationToken = default)
        {
            return _client.Get<GetTransferRequestResponse>($"api/accounts/{transferSenderId}/sender/transfers/{transferRequestId}", null, cancellationToken);
        }

        public Task UpdateTransferRequestForSender(long transferSenderId, long transferRequestId, long cohortId, UpdateTransferApprovalForSenderRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PostAsJson($"api/accounts/{transferSenderId}/transfers/{transferRequestId}/approval/{cohortId}", request, cancellationToken);
        }

        public Task<GetTransferRequestResponse> GetTransferRequestForReceiver(long transferReceiverId, long transferRequestId, CancellationToken cancellationToken = default)
        {
            return _client.Get<GetTransferRequestResponse>($"api/accounts/{transferReceiverId}/receiver/transfers/{transferRequestId}", null, cancellationToken);
        }

        public Task<ValidateUlnOverlapResult> ValidateUlnOverlap(ValidateUlnOverlapRequest validateUlnOverlapRequest, CancellationToken cancellationToken = default)
        {
            return _client.PostAsJson<ValidateUlnOverlapRequest, ValidateUlnOverlapResult>($"api/apprenticeships/uln/validate", validateUlnOverlapRequest, cancellationToken);
        }

        public Task TriageDataLocks(long apprenticeshipId, TriageDataLocksRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PostAsJson($"api/apprenticeships/{apprenticeshipId}/datalocks/triage", request, cancellationToken);
        }

        public Task<GetAllCohortAccountIdsResponse> GetAllCohortAccountIds(CancellationToken cancellationToken = default)
        {
            return _client.Get<GetAllCohortAccountIdsResponse>($"api/cohorts/accountIds");
        }

        public Task<GetEmailOverlapsResponse> GetEmailOverlapChecks(long cohortId, CancellationToken cancellationToken = default)
        {
            return _client.Get<GetEmailOverlapsResponse>($"api/cohorts/{cohortId}/email-overlaps", null, cancellationToken);
        }
    }
}