using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface ICohortDomainService
    {
        Task<Cohort> CreateCohort(long providerId, long accountLegalEntityId,
            DraftApprenticeshipDetails draftApprenticeshipDetails, CancellationToken cancellationToken);

        Task AddDraftApprenticeship(long providerId, long accountLegalEntityId, DraftApprenticeshipDetails draftApprenticeshipDetails, in CancellationToken cancellationToken);
    }
}
