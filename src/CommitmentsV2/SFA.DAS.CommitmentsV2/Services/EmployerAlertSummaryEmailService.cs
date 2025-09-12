using Microsoft.Extensions.Logging;
using NServiceBus;
using Polly;
using Polly.Retry;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types;

namespace SFA.DAS.CommitmentsV2.Services;

public class EmployerAlertSummaryEmailService : IEmployerAlertSummaryEmailService
{
    private readonly IApprenticeshipDomainService _apprenticeshipDomainService;
    private readonly IMessageSession _messageSession;
    private readonly IApprovalsOuterApiClient _approvalsOuterApiClient;
    private readonly ILogger<EmployerAlertSummaryEmailService> _logger;
    private readonly AsyncRetryPolicy _asyncRetryPolicy;
    private readonly CommitmentsV2Configuration _commitmentsV2Configuration;

    public EmployerAlertSummaryEmailService(IApprenticeshipDomainService apprenticeshipDomainService, IMessageSession messageSession, IApprovalsOuterApiClient approvalsOuterApiClient,
        ILogger<EmployerAlertSummaryEmailService> logger, CommitmentsV2Configuration commitmentsV2Configuration)
    {
        _apprenticeshipDomainService = apprenticeshipDomainService;
        _messageSession = messageSession;
        _approvalsOuterApiClient = approvalsOuterApiClient;
        _logger = logger;
        _commitmentsV2Configuration = commitmentsV2Configuration;
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
            .Select(task => task.Result)
            .Where(policy => policy.Outcome == OutcomeType.Successful)
            .Select(policy => policy.Result)
            .Where(response => response != null)
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
                {
                    "total_count_text", alertSummary.TotalCount == 1
                        ? "is 1 apprentice"
                        : $"are {alertSummary.TotalCount} apprentices"
                },
                { "account_name", accountName },
                { "need_needs", alertSummary.TotalCount > 1 ? "need" : "needs" },
                { "changes_for_review", ChangesForReviewText(alertSummary.ChangesForReviewCount) },
                { "requested_changes", RestartRequestText(alertSummary.RestartRequestCount) },
                { "link_to_mange_apprenticeships", $"{_commitmentsV2Configuration.EmployerCommitmentsBaseUrl}/{hashedAccountId}/apprentices" },
                { "link_to_unsubscribe", $"/settings/notifications/unsubscribe/{hashedAccountId}" },
                { "apprentice_request_for_review", RequestsForReviewText(alertSummary.RequestsForReviewCount) }
            };

        _messageSession.Send(new SendEmailToEmployerCommand(accountId, "EmployerAlertSummaryNotification", tokens, null, "name"));
    }

    private static string RestartRequestText(int restartRequestCount)
    {
        return restartRequestCount switch
        {
            0 => string.Empty,
            1 => $"* {restartRequestCount} apprentice with requested changes",
            _ => $"* {restartRequestCount} apprentices with requested changes"
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

    private static string ChangesForReviewText(int changesForReview)
    {
        return changesForReview switch
        {
            0 => string.Empty,
            1 => $"* {changesForReview} apprentice with changes for review",
            _ => $"* {changesForReview} apprentices with changes for review"
        };
    }

    private AsyncRetryPolicy GetRetryPolicy()
    {
        const int maxRetryAttempts = 3;
        var pauseBetweenFailures = TimeSpan.FromSeconds(2);

        return Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(maxRetryAttempts, i => pauseBetweenFailures,
                (exception, retryCount) => { _logger.LogWarning("Error connecting to Approvals Outer Api: ({Message}). Retrying...attempt {retryCount})", exception.Message, retryCount); }
            );
    }
}