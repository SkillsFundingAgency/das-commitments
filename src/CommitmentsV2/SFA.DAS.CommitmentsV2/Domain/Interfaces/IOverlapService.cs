using System;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Domain.Entities;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface IOverlapCheckService
    {
        Task<OverlapCheckResult> CheckForOverlaps(string uln, DateRange range,
            long? existingApprenticeshipId, CancellationToken cancellationToken);
    }
}
