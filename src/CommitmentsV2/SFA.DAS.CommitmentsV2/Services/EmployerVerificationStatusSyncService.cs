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
    private const int CompletionStatusCompleted = 2;

    public async Task SyncPendingEmploymentChecksAsync()
    {
        logger.LogInformation("EmployerVerificationStatusSyncService: starting sync (DB batch {DbBatchSize}, API page {ApiPageSize})", DbBatchSize, ApiPageSize);

        var dbContext = db.Value;
        var now = DateTime.UtcNow;
        var oneDayAgo = now.AddDays(-1);
        var fiveMonthsAgo = now.AddMonths(-5);

        var idsToSync = await dbContext.EmployerVerificationRequests
            .Where(x => x.Created >= fiveMonthsAgo
                && (
                    (x.Updated == null && x.Created <= oneDayAgo)
                    || (x.Updated != null && x.Updated <= oneDayAgo && x.Status != EmployerVerificationRequestStatus.Passed)
                ))
            .OrderBy(x => x.ApprenticeshipId)
            .Take(DbBatchSize)
            .Select(x => x.ApprenticeshipId)
            .ToListAsync();

        if (idsToSync.Count == 0)
        {
            logger.LogInformation("EmployerVerificationStatusSyncService: no records to sync");
            return;
        }

        var requestsByApprenticeshipId = await dbContext.EmployerVerificationRequests
            .Where(x => idsToSync.Contains(x.ApprenticeshipId))
            .ToDictionaryAsync(x => x.ApprenticeshipId);

        var totalUpdated = 0;

        for (var offset = 0; offset < idsToSync.Count; offset += ApiPageSize)
        {
            var pageIds = idsToSync.Skip(offset).Take(ApiPageSize).ToList();
            if (pageIds.Count == 0)
                break;

            logger.LogInformation("EmployerVerificationStatusSyncService: fetching employment checks for page of {Count} apprenticeship IDs (offset {Offset})", pageIds.Count, offset);

            var response = await apiClient.Get<GetEmploymentChecksResponse>(new GetEmploymentChecksRequest(pageIds));
            var checks = response?.Checks ?? [];

            foreach (var check in checks)
            {
                if (!requestsByApprenticeshipId.TryGetValue(check.ApprenticeshipId, out var request))
                    continue;

                request.Status = MapStatus(check);
                request.Notes = MapNotes(check);
                request.LastCheckedDate = check.DateOfCheck;
                request.Updated = now;
                totalUpdated++;
            }
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation("EmployerVerificationStatusSyncService: updated {Updated} employer verification requests from {PageCount} API page(s)", totalUpdated, (idsToSync.Count + ApiPageSize - 1) / ApiPageSize);
    }

    private static EmployerVerificationRequestStatus MapStatus(EvsCheckResponse check)
    {
        var result = check.Result;
        if (result == null)
            return EmployerVerificationRequestStatus.Error;

        if (result.CompletionStatus == CompletionStatusCompleted)
        {
            if (result.Employed == true)
                return EmployerVerificationRequestStatus.Passed;
            if (result.Employed == false)
                return EmployerVerificationRequestStatus.Failed;
        }

        return EmployerVerificationRequestStatus.Error;
    }

    private static string MapNotes(EvsCheckResponse check)
    {
        var result = check.Result;
        return !string.IsNullOrEmpty(result?.ErrorCode) ? result.ErrorCode : null;
    }
}
