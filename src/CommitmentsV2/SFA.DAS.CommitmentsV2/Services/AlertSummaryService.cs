using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NServiceBus;
using Polly;
using Polly.Retry;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class AlertSummaryService : IAlertSummaryService
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly IMessageSession _messageSession;
        private readonly IApprovalsOuterApiClient _approvalsOuterApiClient;
        private readonly IEncodingService _encodingService;
        private readonly ILogger<AlertSummaryService> _logger;
        private readonly AsyncRetryPolicy _asyncRetryPolicy;

        public AlertSummaryService(Lazy<ProviderCommitmentsDbContext> dbContext, IMessageSession messageSession, IApprovalsOuterApiClient approvalsOuterApiClient, 
            IEncodingService encodingService, ILogger<AlertSummaryService> logger)
        {
            _dbContext = dbContext;
            _messageSession = messageSession;
            _approvalsOuterApiClient = approvalsOuterApiClient;
            _encodingService = encodingService;
            _logger = logger;
            _asyncRetryPolicy = GetRetryPolicy();
        }

        public async Task SendEmployerAlertSummaryNotifications()
        { 
            var employerAlertSummaryNotifications = await GetEmployerAlertSummaryNotifications();

            var accountsTasks = employerAlertSummaryNotifications
                .Select(x => _asyncRetryPolicy.ExecuteAndCaptureAsync(() => _approvalsOuterApiClient.Get<AccountResponse>(new GetAccountRequest(x.EmployerHashedAccountId))))
                .ToList();

            await Task.WhenAll(accountsTasks);

            var accounts = accountsTasks
                .Select(p => p.Result)
                .Where(p => p.Outcome == OutcomeType.Successful)
                .Select(p => p.Result)
                .Where(p => p != null)
                .ToList();

            accounts.ForEach(x =>
            {
                var alertSummary = employerAlertSummaryNotifications.Single(a => a.EmployerHashedAccountId == x.HashedAccountId);
                SendEmail(alertSummary, x.AccountId, x.HashedAccountId, x.DasAccountName);
            });
        }

        private async Task<List<EmployerAlertSummaryNotification>> GetEmployerAlertSummaryNotifications()
        {
            _logger.LogInformation("Getting Alert Summaries for employer accounts");

            var queryPendingUpdateByProvider = _dbContext.Value.Apprenticeships
                .Where(app => app.PaymentStatus > 0)
                .Where(app => app.PendingUpdateOriginator == Originator.Provider)
                .GroupBy(app => app.Cohort.EmployerAccountId)
                .Select(m => new { EmployerAccountId = m.Key, PendingUpdateByProviderCount = m.Count() });

            var queryCourseTriaged = _dbContext.Value.Apprenticeships
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

            var queryPriceTriaged = _dbContext.Value.Apprenticeships
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
                    EmployerHashedAccountId = _encodingService.Encode(p, EncodingType.AccountId),
                    TotalCount = pendingUpdateByProvider.GetValueOrDefault(p, 0) + priceTriaged.GetValueOrDefault(p, 0) + courseTriaged.GetValueOrDefault(p, 0),
                    ChangesForReviewCount = pendingUpdateByProvider.GetValueOrDefault(p, 0) + priceTriaged.GetValueOrDefault(p, 0),
                    RestartRequestCount = courseTriaged.GetValueOrDefault(p, 0)
                    
                })
                .ToList();

            if (results.Any())
            {
                _logger.LogInformation("Retrieved Alert Summaries for employer accounts");
            }
            else
            {
                _logger.LogInformation($"Cannot find any Alert Summaries for employer accounts");
            }

            return results;
        }

        private void SendEmail(EmployerAlertSummaryNotification alertSummary, long accountId, string hashedAccountId, string accountName)
        {
            var tokens =
                new Dictionary<string, string>
                    {
                        { "total_count_text", alertSummary.TotalCount == 1
                            ? "is 1 apprentice"
                            : $"are {alertSummary.TotalCount} apprentices" },
                        { "account_name", accountName },
                        { "need_needs", alertSummary.TotalCount > 1 ? "need" :"needs" },
                        { "changes_for_review", ChangesForReviewText(alertSummary.ChangesForReviewCount) },
                        { "requested_changes", RestartRequestText(alertSummary.RestartRequestCount) },
                        { "link_to_mange_apprenticeships", $"accounts/{hashedAccountId}/apprentices/manage/all?RecordStatus=ChangesForReview&RecordStatus=ChangeRequested" },
                        { "link_to_unsubscribe", $"/settings/notifications/unsubscribe/{hashedAccountId}" }
                    };

            _messageSession.Send(new SendEmailToEmployerCommand(accountId, "EmployerAlertSummaryNotification", tokens, null, "name"));
        }

        private string RestartRequestText(int restartRequestCount)
        {
            if (restartRequestCount == 0)
                return string.Empty;

            if (restartRequestCount == 1)
                return $"* {restartRequestCount} apprentice with requested changes";

            return $"* {restartRequestCount} apprentices with requested changes";

        }

        private string ChangesForReviewText(int changesForReview)
        {
            if (changesForReview == 0)
                return string.Empty;

            if (changesForReview == 1)
                return $"* {changesForReview} apprentice with changes for review";

            return $"* {changesForReview} apprentices with changes for review";
        }

        private AsyncRetryPolicy GetRetryPolicy()
        {
            var maxRetryAttempts = 3;
            var pauseBetweenFailures = TimeSpan.FromSeconds(2);

            return Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(maxRetryAttempts, i => pauseBetweenFailures,
                    (exception, retryCount) =>
                    {
                        _logger.LogWarning($"Error connecting to Approvals Outer Api: ({exception.Message}). Retrying...attempt {retryCount})");
                    }
                );
        }
    }
}
