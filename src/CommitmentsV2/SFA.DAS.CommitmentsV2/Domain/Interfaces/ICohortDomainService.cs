using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface ICohortDomainService
    {
        Task<Cohort> CreateCohort(long providerId, long accountLegalEntityId,
            DraftApprenticeshipDetails draftApprenticeshipDetails, CancellationToken cancellationToken);

        Task<Cohort> UpdateDraftApprenticeship(long cohortId, DraftApprenticeshipDetails draftApprenticeshipDetails, CancellationToken cancellationToken);

        Task<DraftApprenticeship> AddDraftApprenticeship(long providerId, long cohortId,
            DraftApprenticeshipDetails draftApprenticeshipDetails, CancellationToken cancellationToken);
    }
}
