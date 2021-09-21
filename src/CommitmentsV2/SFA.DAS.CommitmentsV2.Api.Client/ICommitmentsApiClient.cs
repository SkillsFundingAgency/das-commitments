using System;
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
        Task<AddDraftApprenticeshipResponse> AddDraftApprenticeship(long cohortId, AddDraftApprenticeshipRequest request, CancellationToken cancellationToken = default);
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
        Task UpdateProviderPaymentsPriority(long accountId, UpdateProviderPaymentsPriorityRequest request, CancellationToken cancellationToken = default);
        Task<AccountResponse> GetAccount(long accountId, CancellationToken cancellationToken = default);
        Task<GetApprenticeshipResponse> GetApprenticeship(long apprenticeshipId, CancellationToken cancellationToken = default);
        Task<GetPriceEpisodesResponse> GetPriceEpisodes(long apprenticeshipId, CancellationToken cancellationToken = default);
        Task<GetApprenticeshipUpdatesResponse> GetApprenticeshipUpdates(long apprenticeshipId, GetApprenticeshipUpdatesRequest request, CancellationToken cancellationToken = default);
        Task<GetDataLocksResponse> GetApprenticeshipDatalocksStatus(long apprenticeshipId, CancellationToken cancellationToken = default);
        Task<GetDataLockSummariesResponse> GetApprenticeshipDatalockSummariesStatus(long apprenticeshipId, CancellationToken cancellationToken = default);
        Task AcceptDataLockChanges(long apprenticeshipId, AcceptDataLocksRequestChangesRequest request, CancellationToken cancellationToken = default);
        Task RejectDataLockChanges(long apprenticeshipId, RejectDataLocksRequestChangesRequest request, CancellationToken cancellationToken = default);
        Task CreateChangeOfPartyRequest(long apprenticeshipId, CreateChangeOfPartyRequestRequest request, CancellationToken cancellationToken = default);
        Task<GetChangeOfPartyRequestsResponse> GetChangeOfPartyRequests(long apprenticeshipId, CancellationToken cancellationToken = default);
        Task<GetChangeOfProviderChainResponse> GetChangeOfProviderChain(long apprenticeshipId, CancellationToken cancellationToken = default);
        Task<GetChangeOfEmployerChainResponse> GetChangeOfEmployerChain(long apprenticeshipId, CancellationToken cancellationToken = default);
        Task UpdateEndDateOfCompletedRecord(EditEndDateRequest request, CancellationToken cancellationToken = default);
        Task StopApprenticeship(long apprenticeshipId, StopApprenticeshipRequest request, CancellationToken cancellationToken = default);
        Task PauseApprenticeship(PauseApprenticeshipRequest request, CancellationToken cancellationToken = default);
        Task ResumeApprenticeship(ResumeApprenticeshipRequest request, CancellationToken cancellationToken = default);
        Task<GetAllTrainingProgrammesResponse> GetAllTrainingProgrammes(CancellationToken cancellationToken = default);
        Task<GetAllTrainingProgrammeStandardsResponse> GetAllTrainingProgrammeStandards(CancellationToken cancellationToken = default);
        Task<GetTrainingProgrammeResponse> GetTrainingProgramme(string id, CancellationToken cancellationToken = default);
        Task<GetTrainingProgrammeVersionsResponse> GetTrainingProgrammeVersions(string id, CancellationToken cancellationToken = default);
        Task<GetNewerTrainingProgrammeVersionsResponse> GetNewerTrainingProgrammeVersions(string standardUId, CancellationToken cancellationToken = default);
        Task<GetTrainingProgrammeResponse> GetTrainingProgrammeVersionByStandardUId(string standardUId, CancellationToken cancellationToken = default);
        Task<GetTrainingProgrammeResponse> GetTrainingProgrammeVersionByCourseCodeAndVersion(string courseCode, string version, CancellationToken cancellationToken = default);
        Task<GetTrainingProgrammeResponse> GetCalculatedTrainingProgrammeVersion(int courseCode, DateTime startDate, CancellationToken cancellationToken = default);

        Task UpdateApprenticeshipStopDate(long apprenticeshipId, ApprenticeshipStopDateRequest request, CancellationToken cancellationToken = default);
        Task ValidateApprenticeshipForEdit(ValidateApprenticeshipForEditRequest request, CancellationToken cancellationToken = default);
        Task AcceptApprenticeshipUpdates(long apprenticeshipId, AcceptApprenticeshipUpdatesRequest request, CancellationToken cancellationToken = default);
        Task RejectApprenticeshipUpdates(long apprenticeshipId, RejectApprenticeshipUpdatesRequest request, CancellationToken cancellationToken = default);
        Task UndoApprenticeshipUpdates(long apprenticeshipId, UndoApprenticeshipUpdatesRequest request, CancellationToken cancellationToken = default);
        Task<EditApprenticeshipResponse> EditApprenticeship(EditApprenticeshipApiRequest request, CancellationToken cancellationToken = default);
        Task<GetTransferRequestResponse> GetTransferRequestForSender(long transferSenderId, long transferRequestId, CancellationToken cancellationToken = default);
        Task UpdateTransferRequestForSender(long transferSenderId, long transferRequestId, long cohortId, UpdateTransferApprovalForSenderRequest request, CancellationToken cancellationToken = default);
        Task<GetTransferRequestResponse> GetTransferRequestForReceiver(long transferReceiverId, long transferRequestId, CancellationToken cancellationToken = default);
        Task<ValidateUlnOverlapResult> ValidateUlnOverlap(ValidateUlnOverlapRequest validateUlnOverlapRequest, CancellationToken cancellationToken = default);
        Task TriageDataLocks(long apprenticeshipId, TriageDataLocksRequest request, CancellationToken cancellationToken = default);
        Task<GetAllCohortAccountIdsResponse> GetAllCohortAccountIds(CancellationToken cancellationToken = default);
        Task<GetEmailOverlapsResponse> GetEmailOverlapChecks(long cohortId, CancellationToken cancellationToken = default);
        Task<GetProviderCommitmentAgreementResponse> GetProviderCommitmentAgreement(long providerId, CancellationToken cancellationToken = default);
    }
}