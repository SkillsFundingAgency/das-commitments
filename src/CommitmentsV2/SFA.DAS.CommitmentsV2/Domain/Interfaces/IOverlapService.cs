using System;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Domain.Entities;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface IOverlapCheckService
    {
        Task<OverlapCheckResult> CheckForOverlaps(string uln, DateTime startDate, DateTime endDate,
            long? existingApprenticeshipId, CancellationToken cancellationToken);
    }
}
