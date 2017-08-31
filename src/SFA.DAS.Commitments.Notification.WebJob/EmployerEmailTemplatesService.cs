using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Notification.WebJob.Models;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Notifications.Api.Types;
using Polly;

namespace SFA.DAS.Commitments.Notification.WebJob
{
    public class EmployerEmailTemplatesService : IEmployerEmailTemplatesService
    {
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly IAccountApiClient _accountApi;
        private readonly ILog _logger;
        private readonly Policy _retryPolicy;

        public EmployerEmailTemplatesService(
            IApprenticeshipRepository apprenticeshipRepository,
            IAccountApiClient accountApi,
            ILog logger)
        {
            if (apprenticeshipRepository == null)
                throw new ArgumentNullException($"{nameof(apprenticeshipRepository)} is null");
            if (accountApi == null)
                throw new ArgumentNullException($"{nameof(accountApi)} is null");
            if (logger == null)
                throw new ArgumentNullException($"{nameof(logger)} is null");

            _apprenticeshipRepository = apprenticeshipRepository;
            _accountApi = accountApi;
            _logger = logger;
            _retryPolicy = GetRetryPolicy();
        }

        public async Task<IEnumerable<Email>> GetEmails()
        {
            var alertSummaries = await _apprenticeshipRepository.GetEmployerApprenticeshipAlertSummary();

            _logger.Info($"Found {alertSummaries.Count} employer summary records.");

            var distinctAccountIdsFromAlert =
                alertSummaries
                .Select(m => m.EmployerAccountId)
                .Distinct()
                .ToList();

            var distinctAccountsTasks =
                 distinctAccountIdsFromAlert
                .Select(x => _retryPolicy.ExecuteAndCaptureAsync(() => _accountApi.GetAccount(x)))
                .ToList();

            var userPerAccountTasks =
                 distinctAccountIdsFromAlert
                .Select(ToUserModel)
                .ToList();

            await Task.WhenAll(distinctAccountsTasks);
            await Task.WhenAll(userPerAccountTasks);

            var accounts = distinctAccountsTasks
                .Select(x => x.Result)
                .Where(x => x.Outcome == OutcomeType.Successful)
                .Select(x => x.Result).ToList();

            // Only accountIds where user in DB
            var accountIds = accounts.Select(m => m.AccountId);

            var accountsWithUsers = userPerAccountTasks
                .Select(x => x.Result)
                .Where(u => u.Users != null)
                .Where(x => accountIds.Contains(x.AccountId));

            return accountsWithUsers.SelectMany(m =>
                    {
                        var account = accounts.FirstOrDefault(a => a.AccountId == m.AccountId);
                        var alert = alertSummaries.Single(sum => sum.EmployerAccountId == m.AccountId);

                        return m.Users
                            .Where(u => u.CanReceiveNotifications)
                            .Where(u => u.Role == "Owner" || u.Role == "Transactor")
                            .Select(userModel => MapToEmail(userModel, alert, account.HashedAccountId, account.DasAccountName));
                    }
                );
        }

        private async Task<UserModel> ToUserModel(long accountId)
        {
            var usersResult = await _retryPolicy.ExecuteAndCaptureAsync(() => _accountApi.GetAccountUsers(accountId));
            if (usersResult.Outcome == OutcomeType.Failure )
                _logger.Error(usersResult.FinalException, $"Unable to get employer users for account: {accountId} from account api");
            if (usersResult.Result == null || !usersResult.Result.Any())
            {
                _logger.Warn($"No users found for account: {accountId}");
            }

            return new UserModel
            {
                AccountId = accountId,
                Users = usersResult.Result
            };
        }

        private Email MapToEmail(TeamMemberViewModel userModel, AlertSummary alertSummary, string hashedAccountId, string accountName)
        {
            return new Email
            {
                RecipientsAddress = userModel.Email,
                TemplateId = "EmployerAlertSummaryNotification",
                ReplyToAddress = "digital.apprenticeship.service@notifications.service.gov.uk",
                Subject = "Items for your attention: apprenticeship service",
                SystemId = "x",
                Tokens =
                    new Dictionary<string, string>
                        {
                            { "name", userModel.Name },
                            { "total_count_text", alertSummary.TotalCount == 1 
                                ? "is 1 apprentice" 
                                : $"are {alertSummary.TotalCount} apprentices" },
                            { "account_name", accountName },
                            { "changes_for_review", alertSummary.ChangesForReview > 0 
                                ? $"* {alertSummary.ChangesForReview} with changes for review" 
                                : string.Empty },
                            { "requested_changes", alertSummary.RestartRequestCount > 0 
                                ? $"* {alertSummary.RestartRequestCount} with requested changes" 
                                : string.Empty },
                            { "link_to_mange_apprenticeships", $"accounts/{hashedAccountId}/apprentices/manage/all?RecordStatus=ChangesForReview&RecordStatus=ChangeRequested" },
                            { "link_to_unsubscribe", $"/settings/notifications/unsubscribe/{hashedAccountId}" }
                        }
                };
        }

        private Polly.Retry.RetryPolicy GetRetryPolicy()
        {
            return Policy
                    .Handle<Exception>()
                    .RetryAsync(3,
                        (exception, retryCount) =>
                        {
                            _logger.Warn($"Error connecting to EAS Account Api: ({exception.Message}). Retrying...attempt {retryCount})");
                        }
                    );
        }
    }
}
