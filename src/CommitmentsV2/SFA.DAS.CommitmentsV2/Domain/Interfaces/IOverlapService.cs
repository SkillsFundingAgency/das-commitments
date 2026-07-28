using SFA.DAS.CommitmentsV2.Domain.Entities;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces;

public interface IOverlapCheckService
{
    Task<OverlapCheckResult> CheckForOverlaps(string uln, CourseDateRange range,
        long? existingApprenticeshipId, CancellationToken cancellationToken);

    Task<List<OverlapCheckResult>> CheckForOverlaps(long cohortId, CancellationToken cancellationToken);

    Task<EmailOverlapCheckResult> CheckForEmailOverlaps(string email, CourseDateRange range,
        long? existingApprenticeshipId, long? cohortId, CancellationToken cancellationToken);

    Task<List<EmailOverlapCheckResult>> CheckForEmailOverlaps(long cohortId, CancellationToken cancellationToken);

    Task<OverlapCheckResultOnStartDate> CheckForOverlapsOnStartDate(string uln, CourseDateRange range,
        long? existingApprenticeshipId, CancellationToken cancellationToken);
}