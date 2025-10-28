using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.Services;

public class ApprenticeshipDomainService(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    IEncodingService encodingService,
    ILogger<ApprenticeshipDomainService> logger)
    : IApprenticeshipDomainService
{
    public async Task<List<EmployerAlertSummaryNotification>> GetEmployerAlertSummaryNotifications()
    {
        logger.LogInformation("Getting Alert Summaries for employer accounts");

        var queryPendingUpdateByProvider = dbContext.Value.Apprenticeships
            .Where(app => app.PaymentStatus > 0)
            .Where(app => app.PendingUpdateOriginator == Originator.Provider)
            .GroupBy(app => app.Cohort.EmployerAccountId)
            .Select(m => new { EmployerAccountId = m.Key, PendingUpdateByProviderCount = m.Count() });

        var queryCourseTriaged = dbContext.Value.Apprenticeships
            .Where(app => app.PaymentStatus > 0)
            .Where(app => app.DataLockStatus.Any(dlock =>
                dlock.IsResolved == false &&
                dlock.IsExpired == false &&
                dlock.Status == Status.Fail &&
                dlock.EventStatus != EventStatus.Removed &&
                dlock.TriageStatus == TriageStatus.Restart &&
                (dlock.ErrorCode.HasFlag(DataLockErrorCode.Dlock03) ||
                 dlock.ErrorCode.HasFlag(DataLockErrorCode.Dlock04) ||
                 dlock.ErrorCode.HasFlag(DataLockErrorCode.Dlock05) ||
                 dlock.ErrorCode.HasFlag(DataLockErrorCode.Dlock06))))
            .GroupBy(app => app.Cohort.EmployerAccountId)
            .Select(m => new { EmployerAccountId = m.Key, RestartRequestCount = m.Count() });

        var queryPriceTriaged = dbContext.Value.Apprenticeships
            .Where(app => app.PaymentStatus > 0)
            .Where(app => app.DataLockStatus.Any(dlock =>
                dlock.IsResolved == false &&
                dlock.IsExpired == false &&
                dlock.Status == Status.Fail &&
                dlock.EventStatus != EventStatus.Removed &&
                dlock.TriageStatus == TriageStatus.Change &&
                dlock.ErrorCode.HasFlag(DataLockErrorCode.Dlock07)))
            .GroupBy(app => app.Cohort.EmployerAccountId)
            .Select(m => new { EmployerAccountId = m.Key, ChangesForReviewCount = m.Count() });

        var pendingUpdateByProvider = await queryPendingUpdateByProvider.ToDictionaryAsync(p => p.EmployerAccountId, p => p.PendingUpdateByProviderCount);
        var courseTriaged = await queryCourseTriaged.ToDictionaryAsync(p => p.EmployerAccountId, p => p.RestartRequestCount);
        var priceTriaged = await queryPriceTriaged.ToDictionaryAsync(p => p.EmployerAccountId, p => p.ChangesForReviewCount);

        var results = pendingUpdateByProvider.Select(p => p.Key).Union(courseTriaged.Select(p => p.Key).Union(priceTriaged.Select(p => p.Key)))
            .Distinct()
            .Select(p => new EmployerAlertSummaryNotification
            {
                EmployerHashedAccountId = encodingService.Encode(p, EncodingType.AccountId),
                TotalCount = pendingUpdateByProvider.GetValueOrDefault(p, 0) + priceTriaged.GetValueOrDefault(p, 0) + courseTriaged.GetValueOrDefault(p, 0),
                ChangesForReviewCount = pendingUpdateByProvider.GetValueOrDefault(p, 0) + priceTriaged.GetValueOrDefault(p, 0),
                RestartRequestCount = courseTriaged.GetValueOrDefault(p, 0)
            })
            .ToList();

        if (results.Count == 0)
        {
            logger.LogInformation("Cannot find any Alert Summaries for employer accounts");
        }
        else
        {
            logger.LogInformation("Retrieved Alert Summaries for employer accounts");
        }

        return results;
    }
}