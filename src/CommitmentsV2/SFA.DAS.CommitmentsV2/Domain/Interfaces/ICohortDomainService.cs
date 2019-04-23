using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Domain.ValueObjects;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface ICohortDomainService
    {
        Task<Commitment> CreateCohort(long providerId, long accountLegalEntityId,
            DraftApprenticeshipDetails draftApprenticeshipDetails, CancellationToken cancellationToken);
    }
}
