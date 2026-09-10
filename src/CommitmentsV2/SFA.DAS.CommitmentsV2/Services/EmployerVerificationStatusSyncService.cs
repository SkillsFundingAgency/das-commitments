using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Services;

public class EmployerVerificationStatusSyncService(
    Lazy<ProviderCommitmentsDbContext> db,
    IApprovalsOuterApiClient apiClient,
    ILogger<EmployerVerificationStatusSyncService> logger) : IEmployerVerificationStatusSyncService
{
    private const int DbBatchSize = 1000;
    private const int ApiPageSize = 50;
    private const int MaxDbBatchesPerRun = 200;

    public async Task SyncPendingEmploymentChecksAsync()
    {
        logger.LogInformation("EmployerVerificationStatusSyncService: starting sync (DB batch {DbBatchSize}, API page {ApiPageSize}, max batches {MaxDbBatchesPerRun})", DbBatchSize, ApiPageSize, MaxDbBatchesPerRun);

        var dbContext = db.Value;
        var now = DateTime.UtcNow;
        var oneDayAgo = now.AddDays(-1);
        var fiveMonthsAgo = now.AddMonths(-5);
        var totalUpdated = 0;
        var batchesProcessed = 0;

        for (var batch = 0; batch < MaxDbBatchesPerRun; batch++)
        {
            var idsToSync = await dbContext.EmployerVerificationRequests
                .Where(x => x.Created >= fiveMonthsAgo
                    && (
                        (x.Updated == null && x.Created <= oneDayAgo)
                        || (x.Updated != null && x.Updated <= oneDayAgo && x.Employed != true)
                    ))
                .OrderBy(x => x.ApprenticeshipId)
                .Take(DbBatchSize)
                .Select(x => x.ApprenticeshipId)
                .ToListAsync();

            if (idsToSync.Count == 0)
            {
                if (batch == 0)
                    logger.LogInformation("EmployerVerificationStatusSyncService: no records to sync");
                break;
            }

            batchesProcessed++;

            var requestsByApprenticeshipId = await dbContext.EmployerVerificationRequests
                .Where(x => idsToSync.Contains(x.ApprenticeshipId))
                .ToDictionaryAsync(x => x.ApprenticeshipId);

            for (var offset = 0; offset < idsToSync.Count; offset += ApiPageSize)
            {
                var pageIds = idsToSync.Skip(offset).Take(ApiPageSize).ToList();
                if (pageIds.Count == 0)
                    break;

                logger.LogInformation("EmployerVerificationStatusSyncService: fetching employment checks for page of {Count} apprenticeship IDs (offset {Offset}, batch {Batch})", pageIds.Count, offset, batch);

                var response = await apiClient.Get<GetEmploymentChecksResponse>(new GetEmploymentChecksRequest(pageIds));
                var checksByApprenticeshipId = (response?.Checks ?? [])
                    .GroupBy(c => c.ApprenticeshipId)
                    .ToDictionary(g => g.Key, g => g.First());

                foreach (var apprenticeshipId in pageIds)
                {
                    if (!requestsByApprenticeshipId.TryGetValue(apprenticeshipId, out var request))
                        continue;

                    request.Updated = now;
                    totalUpdated++;

                    if (!checksByApprenticeshipId.TryGetValue(apprenticeshipId, out var check))
                        continue;

                    request.Employed = check.Result?.Employed;
                    request.Status = MapStatus(check);
                    request.Notes = MapNotes(check);
                    request.LastCheckedDate = check.DateOfCheck;
                }
            }

            await dbContext.SaveChangesAsync();
        }

        logger.LogInformation("EmployerVerificationStatusSyncService: updated {Updated} employer verification requests across {Batches} DB batch(es)", totalUpdated, batchesProcessed);

        if (batchesProcessed == MaxDbBatchesPerRun)
        {
            var remaining = await dbContext.EmployerVerificationRequests
                .CountAsync(x => x.Created >= fiveMonthsAgo
                    && (
                        (x.Updated == null && x.Created <= oneDayAgo)
                        || (x.Updated != null && x.Updated <= oneDayAgo && x.Employed != true)
                    ));
            if (remaining > 0)
            {
                logger.LogWarning("EmployerVerificationStatusSyncService: {Remaining} eligible records were not processed this run and will be retried on the next schedule so daily 5-month checks are not dropped", remaining);
            }
        }
    }

    private static EmployerVerificationRequestStatus MapStatus(EvsCheckResponse check)
    {
        var result = check.Result;
        if (result == null)
            return EmployerVerificationRequestStatus.Error;

        if (result.Employed == true)
            return EmployerVerificationRequestStatus.Passed;
        if (result.Employed == false)
            return EmployerVerificationRequestStatus.Failed;

        return EmployerVerificationRequestStatus.Error;
    }

    private static string MapNotes(EvsCheckResponse check)
    {
        var result = check.Result;
        return !string.IsNullOrEmpty(result?.ErrorCode) ? result.ErrorCode : null;
    }
}
