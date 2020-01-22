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
        Task<GetDraftApprenticeshipResponse> GetDraftApprenticeship(long cohortId, long apprenticeshipId, CancellationToken cancellationToken = default);
        Task<GetDraftApprenticeshipsResponse> GetDraftApprenticeships(long cohortId, CancellationToken cancellationToken = default);
        Task<GetCohortResponse> GetCohort(long cohortId, CancellationToken cancellationToken = default);
        Task<AccountLegalEntityResponse> GetLegalEntity(long accountLegalEntityId, CancellationToken cancellationToken = default);
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
        Task<AccountResponse> GetAccount(long accountId, CancellationToken cancellationToken = default);
    }
}
