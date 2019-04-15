using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Domain.ValueObjects;
using SFA.DAS.CommitmentsV2.Services;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface IApprenticeshipOverlapService
    {
        Task<OverlapStatus> CheckForOverlaps(DraftApprenticeshipDetails draftApprenticeshipDetails, CancellationToken cancellationToken);
    }
}