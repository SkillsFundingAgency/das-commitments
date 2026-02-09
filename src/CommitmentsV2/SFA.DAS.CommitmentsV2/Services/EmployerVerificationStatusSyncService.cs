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
    private const int ApiPageSize = 100;
    private const int CompletionStatusCompleted = 2;

    public async Task SyncPendingEmploymentChecksAsync()
    {
        logger.LogInformation("EmployerVerificationStatusSyncService: starting sync of pending employment checks (DB batch {DbBatchSize}, API page {ApiPageSize})", DbBatchSize, ApiPageSize);

        var dbContext = db.Value;

        var pendingIds = await dbContext.EmployerVerificationRequests
            .Where(x => x.Status == EmployerVerificationRequestStatus.Pending)
            .OrderBy(x => x.ApprenticeshipId)
            .Take(DbBatchSize)
            .Select(x => x.ApprenticeshipId)
            .ToListAsync();

        if (pendingIds.Count == 0)
        {
            logger.LogInformation("EmployerVerificationStatusSyncService: no pending requests");
            return;
        }

        var requestsByApprenticeshipId = await dbContext.EmployerVerificationRequests
            .Where(x => pendingIds.Contains(x.ApprenticeshipId))
            .ToDictionaryAsync(x => x.ApprenticeshipId);

        var now = DateTime.UtcNow;
        var totalUpdated = 0;

        for (var offset = 0; offset < pendingIds.Count; offset += ApiPageSize)
        {
            var pageIds = pendingIds.Skip(offset).Take(ApiPageSize).ToList();
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
        logger.LogInformation("EmployerVerificationStatusSyncService: updated {Updated} employer verification requests from {PageCount} API page(s)", totalUpdated, (pendingIds.Count + ApiPageSize - 1) / ApiPageSize);
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
