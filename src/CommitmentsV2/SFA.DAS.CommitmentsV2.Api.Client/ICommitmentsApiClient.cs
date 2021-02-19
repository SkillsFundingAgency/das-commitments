using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Client
{
    public interface ICommitmentsApiClient
    {
        Task Ping();
        Task<WhoAmIResponse> WhoAmI();
        Task AddDraftApprenticeship(long cohortId, AddDraftApprenticeshipRequest request, CancellationToken cancellationToken = default);
        Task ApproveCohort(long cohortId, ApproveCohortRequest request, CancellationToken cancellationToken = default);
        Task<CreateCohortResponse> CreateCohort(CreateCohortRequest request, CancellationToken cancellationToken = default);
        Task<CreateCohortResponse> CreateCohort(CreateEmptyCohortRequest request, CancellationToken cancellationToken = default);
        Task<CreateCohortResponse> CreateCohort(CreateCohortWithOtherPartyRequest request, CancellationToken cancellationToken = default);
        Task<GetAllProvidersResponse> GetAllProviders(CancellationToken cancellationToken = default);
        Task<GetDraftApprenticeshipResponse> GetDraftApprenticeship(long cohortId, long apprenticeshipId, CancellationToken cancellationToken = default);
        Task<GetDraftApprenticeshipsResponse> GetDraftApprenticeships(long cohortId, CancellationToken cancellationToken = default);
        Task<GetApprenticeshipsResponse> GetApprenticeships(GetApprenticeshipsRequest request, CancellationToken cancellationToken = default);
        Task<GetApprenticeshipsFilterValuesResponse> GetApprenticeshipsFilterValues(GetApprenticeshipFiltersRequest request, CancellationToken cancellationToken = default);
        Task<GetCohortResponse> GetCohort(long cohortId, CancellationToken cancellationToken = default);
        Task<AccountLegalEntityResponse> GetAccountLegalEntity(long accountLegalEntityId, CancellationToken cancellationToken = default);
        Task<GetProviderResponse> GetProvider(long providerId, CancellationToken cancellationToken = default);
        Task<string> SecureCheck();
        Task<string> SecureEmployerCheck();
        Task<string> SecureProviderCheck();
        Task SendCohort(long cohortId, SendCohortRequest request, CancellationToken cancellationToken = default);
        Task UpdateDraftApprenticeship(long cohortId, long apprenticeshipId, UpdateDraftApprenticeshipRequest request, CancellationToken cancellationToken = default);
        Task<bool> IsAgreementSigned(AgreementSignedRequest request, CancellationToken cancellationToken = default);
        Task<long?> GetLatestAgreementId(long accountLegalEntityId, CancellationToken cancellationToken = default);
        Task<GetCohortsResponse> GetCohorts(GetCohortsRequest request, CancellationToken cancellationToken = default);
        Task DeleteDraftApprenticeship(long cohortId, long apprenticeshipId, DeleteDraftApprenticeshipRequest request, CancellationToken cancellationToken = default);
		Task DeleteCohort(long cohortId, UserInfo userInfo, CancellationToken cancellationToken = default);
        Task<GetApprovedProvidersResponse> GetApprovedProviders(long accountId, CancellationToken cancellationToken);
        Task<GetProviderPaymentsPriorityResponse> GetProviderPaymentsPriority(long accountId, CancellationToken cancellationToken = default);
        Task<AccountResponse> GetAccount(long accountId, CancellationToken cancellationToken = default);
        Task<GetApprenticeshipResponse> GetApprenticeship(long apprenticeshipId, CancellationToken cancellationToken = default);
        Task<GetPriceEpisodesResponse> GetPriceEpisodes(long apprenticeshipId, CancellationToken cancellationToken = default);
        Task<GetApprenticeshipUpdatesResponse> GetApprenticeshipUpdates(long apprenticeshipId, GetApprenticeshipUpdatesRequest request, CancellationToken cancellationToken = default);
        Task<GetDataLocksResponse> GetApprenticeshipDatalocksStatus(long apprenticeshipId, CancellationToken cancellationToken = default);
        Task CreateChangeOfPartyRequest(long apprenticeshipId, CreateChangeOfPartyRequestRequest request, CancellationToken cancellationToken = default);
        Task<GetChangeOfPartyRequestsResponse> GetChangeOfPartyRequests(long apprenticeshipId, CancellationToken cancellationToken = default);
        Task UpdateEndDateOfCompletedRecord(EditEndDateRequest request, CancellationToken cancellationToken = default);
        Task StopApprenticeship(long apprenticeshipId, StopApprenticeshipRequest request, CancellationToken cancellationToken = default);
        Task PauseApprenticeship(PauseApprenticeshipRequest request, CancellationToken cancellationToken = default);
        Task ResumeApprenticeship(ResumeApprenticeshipRequest request, CancellationToken cancellationToken = default);
        Task<GetAllTrainingProgrammesResponse> GetAllTrainingProgrammes(CancellationToken cancellationToken = default);
        Task<GetAllTrainingProgrammeStandardsResponse> GetAllTrainingProgrammeStandards(CancellationToken cancellationToken = default);
        Task<GetTrainingProgrammeResponse> GetTrainingProgramme(string id, CancellationToken cancellationToken = default);
    }
}
