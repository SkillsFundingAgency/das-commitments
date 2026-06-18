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

        accounts.ForEach(async x =>
        {
            var alertSummary = employerAlertSummaryNotifications.Single(a => a.EmployerHashedAccountId == x.HashedAccountId);
            await SendEmail(alertSummary, x.AccountId, x.HashedAccountId);
        });
    }

    private async Task SendEmail(EmployerAlertSummaryNotification alertSummary, long accountId, string hashedAccountId)
    {
        var tokens =
            new Dictionary<string, string>
            {
                {
                    "total_count_text", alertSummary.TotalCount == 1
                        ? "You have 1 item that needs your attention"
                        : $"You have {alertSummary.TotalCount} items that need your attention"
                },
                { "changes_for_review", ChangesForReviewText(alertSummary.ChangesForReviewCount) },
                { "requested_changes", RestartRequestText(alertSummary.RestartRequestCount) },
                { "ilrchanges_to_confirm", IlrChangesToConfirmText(alertSummary.PendingIlrChangesCount) },
                { "link_to_mange_apprenticeships", $"<a href=\"{_commitmentsV2Configuration.EmployerCommitmentsBaseUrl}/{hashedAccountId}/apprentices\">Sign into your Apprenticeship Service Account</a>"  },
                { "link_to_unsubscribe", $"/settings/notifications/unsubscribe/{hashedAccountId}" }
            };

        await _messageSession.Send(new SendEmailToEmployerCommand(accountId, "EmployerAlertSummaryNotification", tokens, null, "name"));
    }

    private static string RestartRequestText(int restartRequestCount)
    {
        return restartRequestCount switch
        {
            0 => string.Empty,
            1 => $"* {restartRequestCount} learner request to review",
            _ => $"* {restartRequestCount} learner requests to review",
        };
    }

    private static string ChangesForReviewText(int changesForReview)
    {
        return changesForReview switch
        {
            0 => string.Empty,
            1 => $"* {changesForReview} learner with changes for review",
            _ => $"* {changesForReview} learners with changes for review"
        };
    }

    private string IlrChangesToConfirmText(int changestoConfirm)
    {
        if(!_commitmentsV2Configuration.CoCApprovalsActive)
        {
            return string.Empty;
        }

        return changestoConfirm switch
        {
            0 => string.Empty,
            1 => $"* {changestoConfirm} learner with changes from ILR to confirm",
            _ => $"* {changestoConfirm} learners with changes from ILR to confirm",
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