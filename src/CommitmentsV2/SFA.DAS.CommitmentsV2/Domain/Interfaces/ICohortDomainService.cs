using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface ICohortDomainService
    {
        Task<DraftApprenticeship> AddDraftApprenticeship(long providerId, long cohortId, DraftApprenticeshipDetails draftApprenticeshipDetails, UserInfo userInfo, CancellationToken cancellationToken);
        Task<DraftApprenticeship> AddAndApproveDraftApprenticeship(long providerId, long cohortId, DraftApprenticeshipDetails draftApprenticeshipDetails, UserInfo userInfo, CancellationToken cancellationToken);
        Task ApproveCohort(long cohortId, string message, UserInfo userInfo, CancellationToken cancellationToken);
        Task<Cohort> CreateCohort(long providerId, long accountId, long accountLegalEntityId, long? transferSenderId, int? pledgeApplicationId, DraftApprenticeshipDetails draftApprenticeshipDetails, UserInfo userInfo, CancellationToken cancellationToken);
        Task<Cohort> CreateCohortWithOtherParty(long providerId, long accountId, long accountLegalEntityId, long? transferSenderId, int? pledgeApplicationId, string message, UserInfo userInfo, CancellationToken cancellationToken);
        Task<Cohort> CreateEmptyCohort(long providerId, long accountId, long accountLegalEntityId, UserInfo userInfo, CancellationToken cancellationToken);
        Task SendCohortToOtherParty(long cohortId, string message, UserInfo userInfo, CancellationToken cancellationToken);
        Task<Cohort> UpdateDraftApprenticeship(long cohortId, DraftApprenticeshipDetails draftApprenticeshipDetails, UserInfo userInfo, CancellationToken cancellationToken);
        Task<Cohort> DeleteDraftApprenticeship(long cohortId, long apprenticeshipId, UserInfo userInfo, CancellationToken cancellationToken);
        Task<BulkUploadAddDraftApprenticeshipsResponse> GetCohortDetails(long cohortId, CancellationToken cancellationToken);
    }
}