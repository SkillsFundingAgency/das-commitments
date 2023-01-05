using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface ICohortDomainService
    {
        Task<DraftApprenticeship> AddDraftApprenticeship(long providerId, long cohortId, DraftApprenticeshipDetails draftApprenticeshipDetails, UserInfo userInfo, CancellationToken cancellationToken);
        Task ApproveCohort(long cohortId, string message, UserInfo userInfo, Party? requestingParty, CancellationToken cancellationToken);
        Task<Cohort> CreateCohort(long providerId, long accountId, long accountLegalEntityId, long? transferSenderId, int? pledgeApplicationId, DraftApprenticeshipDetails draftApprenticeshipDetails, UserInfo userInfo, CancellationToken cancellationToken);
        Task<Cohort> CreateCohortWithOtherParty(long providerId, long accountId, long accountLegalEntityId, long? transferSenderId, int? pledgeApplicationId, string message, UserInfo userInfo, CancellationToken cancellationToken);
        Task<Cohort> CreateEmptyCohort(long providerId, long accountId, long accountLegalEntityId, UserInfo userInfo, CancellationToken cancellationToken);
        Task SendCohortToOtherParty(long cohortId, string message, UserInfo userInfo, CancellationToken cancellationToken);
        Task<Cohort> UpdateDraftApprenticeship(long cohortId, DraftApprenticeshipDetails draftApprenticeshipDetails, UserInfo userInfo, CancellationToken cancellationToken);
        Task<Cohort> DeleteDraftApprenticeship(long cohortId, long apprenticeshipId, UserInfo userInfo, CancellationToken cancellationToken);
        Task<IEnumerable<Cohort>> AddDraftApprenticeships(List<DraftApprenticeshipDetails> draftApprenticeships, List<BulkUploadAddDraftApprenticeshipRequest> csvBulkUploadApprenticehips, long providerId, UserInfo userInfo, CancellationToken cancellationToken);
        Task ValidateDraftApprenticeshipForOverlappingTrainingDateRequest(long providerId, long? cohortId, DraftApprenticeshipDetails draftApprenticeshipDetails, CancellationToken cancellationToken);
    }
}