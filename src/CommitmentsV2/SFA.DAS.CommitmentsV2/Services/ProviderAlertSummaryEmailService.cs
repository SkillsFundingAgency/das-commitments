using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using System.Diagnostics;
using System.Linq.Expressions;

namespace SFA.DAS.CommitmentsV2.Services;

public class ProviderAlertSummaryEmailService(
    IProviderCommitmentsDbContext context,
    ILogger<ProviderAlertSummaryEmailService> logger,
    CommitmentsV2Configuration commitmentsV2Configuration,
    IMessageSession messageSession)
    : IProviderAlertSummaryEmailService
{
    public async Task SendAlertSummaryEmails(string jobId)
    {
        var alertSummaries = await GetProviderApprenticeshipAlertSummary();

        logger.LogInformation("Found {Count} provider summary records.", alertSummaries.Count);

        if (alertSummaries.Count == 0)
        {
            return;
        }

        var distinctProviderIds = alertSummaries
            .Select(m => m.ProviderId)
            .Distinct()
            .ToList();

        var stopwatch = Stopwatch.StartNew();

        logger.LogDebug("About to send emails to {Count} providers, JobId: {JobId}",distinctProviderIds.Count, jobId);

        var sendEmailTasks = distinctProviderIds.Select(x => SendEmails(x, alertSummaries));
        
        await Task.WhenAll(sendEmailTasks);

        logger.LogDebug("Took {ElapsedMilliseconds} milliseconds to send {Count} emails, JobId; {JobId}", stopwatch.ElapsedMilliseconds, distinctProviderIds.Count, jobId);
    }

    private async Task SendEmails(long providerId, IList<ProviderAlertSummary> alertSummaries)
    {
        var alert = alertSummaries.First(m => m.ProviderId == providerId);

        var sendEmailToProviderCommand = new SendEmailToProviderCommand(
            providerId,
            "ProviderAlertSummaryNotification2", 
            new Dictionary<string, string>
            {
                {"total_count_text", alert.TotalCount.ToString()},
                {"need_needs", alert.TotalCount > 1 ? "need" : "needs"},
                {"changes_for_review", ChangesForReviewText(alert.ChangesForReview)},
                {"mismatch_changes", GetMismatchText(alert.DataMismatchCount)},
                {
                    "link_to_mange_apprenticeships",
                    $"{commitmentsV2Configuration.ProviderCommitmentsBaseUrl}{providerId}/apprentices"
                },
                { "apprentice_request_for_review", RequestsForReviewText(alert.RequestsForReviewCount) }
            });

        await messageSession.Send(sendEmailToProviderCommand);
    }

    private static string GetMismatchText(int dataLockCount)
    {
        return dataLockCount switch
        {
            0 => string.Empty,
            1 => "* 1 apprentice with an ILR data mismatch",
            _ => $"* {dataLockCount} apprentices with an ILR data mismatch"
        };
    }

    private static string ChangesForReviewText(int changesForReview)
    {
        return changesForReview switch
        {
            0 => string.Empty,
            1 => "* 1 apprentice with changes for review",
            _ => $"* {changesForReview} apprentices with changes for review"
        };
    }

    private static string RequestsForReviewText(int requestsForReviewCount)
    {
        return requestsForReviewCount switch
        {
            0 => string.Empty,
            1 => $"* {requestsForReviewCount} apprentice request to review",
            _ => $"* {requestsForReviewCount} apprentices requests to review"
        };
    }

    private async Task<List<ProviderAlertSummary>> GetProviderApprenticeshipAlertSummary()
    {
        var summaries = new List<ProviderAlertSummary>();

        var providerSummaryInfos = await context.Apprenticeships
            .Where(app => app.Cohort.Provider != null && app.Cohort.AccountLegalEntity != null)
            .Where(app => app.PaymentStatus > 0)
            .Where(app => app.PendingUpdateOriginator == Originator.Employer ||
                          app.DataLockStatus.AsQueryable().Any(_unhandledCourseOrPriceDlock))
            .Select(app => new
            {
                ProviderId = app.Cohort.ProviderId,
                ProviderName = app.Cohort.Provider.Name,
                PendingOriginator = app.PendingUpdateOriginator,
                DLocks = app.DataLockStatus
            }).ToListAsync();

        var cohortReviewStatusCount = context.Cohorts.Where(c => !c.IsDraft && c.WithParty == Party.Provider).GroupBy(p => p.ProviderId)
            .Select(t => new { ProviderId = t.Key, RequestsForReviewCount = t.Count() });

        var reviewCount = await cohortReviewStatusCount.ToDictionaryAsync(p => p.ProviderId, p => p.RequestsForReviewCount);

        var providerGroups = providerSummaryInfos.GroupBy(app => app.ProviderId);

        foreach (var providerGroup in providerGroups)
        {
            var changesForReview = providerGroup.Count(app => app.PendingOriginator == Originator.Employer);

            var dataMismatchCount = providerGroup.Count(app => app.DLocks.Any(
                dlock => dlock.UnHandled() &&
                         (dlock.WithCourseError() || dlock.IsPriceOnly())));

            summaries.Add(new ProviderAlertSummary
            {
                ProviderId = providerGroup.First().ProviderId,
                ProviderName = providerGroup.First().ProviderName,
                TotalCount = changesForReview + dataMismatchCount,
                ChangesForReview = changesForReview,
                DataMismatchCount = dataMismatchCount,
                RequestsForReviewCount = reviewCount.Where(t => t.Key == providerGroup.First().ProviderId).First().Value
            });
        }

        return summaries;
    }

    private readonly Expression<Func<DataLockStatus, bool>> _unhandledCourseOrPriceDlock = dl =>
        !dl.IsResolved && dl.Status != Status.Pass && !dl.IsExpired && dl.TriageStatus == TriageStatus.Unknown
        && (dl.ErrorCode.HasFlag(DataLockErrorCode.Dlock03)
            || dl.ErrorCode.HasFlag(DataLockErrorCode.Dlock04)
            || dl.ErrorCode.HasFlag(DataLockErrorCode.Dlock05)
            || dl.ErrorCode.HasFlag(DataLockErrorCode.Dlock06)
            || dl.ErrorCode.HasFlag(DataLockErrorCode.Dlock07));

}