using Microsoft.Extensions.Logging;
using NServiceBus;
using Polly;
using Polly.Retry;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Infrastructure;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class AlertSummaryService : IAlertSummaryService
    {
        private readonly IApprenticeshipDomainService _apprenticeshipDomainService;
        private readonly IMessageSession _messageSession;
        private readonly IApprovalsOuterApiClient _approvalsOuterApiClient;
        private readonly ILogger<AlertSummaryService> _logger;
        private readonly AsyncRetryPolicy _asyncRetryPolicy;

        public AlertSummaryService(IApprenticeshipDomainService apprenticeshipDomainService, IMessageSession messageSession, IApprovalsOuterApiClient approvalsOuterApiClient, 
            ILogger<AlertSummaryService> logger)
        {
            _apprenticeshipDomainService = apprenticeshipDomainService;
            _messageSession = messageSession;
            _approvalsOuterApiClient = approvalsOuterApiClient;
            _logger = logger;
            _asyncRetryPolicy = GetRetryPolicy();
        }

        public async Task SendEmployerAlertSummaryNotifications()
        { 
            var employerAlertSummaryNotifications = await _apprenticeshipDomainService.GetEmployerAlertSummaryNotifications();

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
