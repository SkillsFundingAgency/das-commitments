using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Notification.WebJob.Models;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Notifications.Api.Types;
using Polly;

namespace SFA.DAS.Commitments.Notification.WebJob
{
    public class EmailTemplatesService : IEmailTemplatesService
    {
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly IAccountApiClient _accountApi;
        private readonly ILog _logger;
        private readonly Policy _retryPolicy;

        public EmailTemplatesService(
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

            _logger.Info($"Found {alertSummaries.Count} summary records.");

            var distinctAccountIds =
                alertSummaries
                .Select(m => m.EmployerAccountId)
                .Distinct()
                .ToList();

            var distinctAccountsTasks =
                 distinctAccountIds
                .Select(x => _retryPolicy.ExecuteAsync(() => _accountApi.GetAccount((long)x)))
                .ToList();

            var userPerAccountTasks =
                 distinctAccountIds
                .Select(ToUserModel)
                .ToList();

            await Task.WhenAll(distinctAccountsTasks);
            await Task.WhenAll(userPerAccountTasks);

            var accounts = distinctAccountsTasks.Select(x => x.Result).ToList();

            var accountsWithUsers = userPerAccountTasks
                                        .Select(x => x.Result)
                                        .Where(u => u.Users != null);

            return accountsWithUsers.SelectMany(m =>
                    {
                        var account = accounts.Single(a => a.AccountId == m.AccountId);
                        
                        var alert = alertSummaries.Single(sum => sum.EmployerAccountId == m.AccountId);

                        return m.Users
                            .Where(u => u.CanReceiveNotifications)
                            .Select(userModel => MapToEmail(userModel, alert, account.HashedAccountId, account.DasAccountName));
                    }
                );
        }

        private async Task<UserModel> ToUserModel(long accountId)
        {
            ICollection<TeamMemberViewModel> users = null;

            try
            {
                users = await _retryPolicy.ExecuteAsync(() => _accountApi.GetAccountUsers(accountId));

                if (users == null || !users.Any())
                    _logger.Warn($"No users found for account: {accountId}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Unable to get users for account: {accountId} from account api");
            }

            return new UserModel
                       {
                           AccountId = accountId,
                           Users = users
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
                                       { "link_to_mange_apprenticeships", $"accounts/{hashedAccountId}/apprentices/manage/all?RecordStatus=ChangesForReview&RecordStatus=ChangeRequested" }
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
                            _logger.Warn($"Error connecting to Account Api: ({exception.Message}). Retrying...attempt {retryCount})");
                        }
                    );
        }
    }
}
