using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Notification.WebJob.Models;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Notifications.Api.Types;

namespace SFA.DAS.Commitments.Notification.WebJob.EmailServices
{
    public class SendingEmployerTransferRequestEmailService : ISendingEmployerTransferRequestEmailService
    {
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly IAccountApiClient _accountApi;
        private readonly ILog _logger;
        private readonly RetryPolicy _retryPolicy;

        public SendingEmployerTransferRequestEmailService(ICommitmentRepository commitmentRepository,
            IAccountApiClient accountApi,
            ILog logger)
        {
            _commitmentRepository = commitmentRepository;
            _accountApi = accountApi;
            _logger = logger;
            _retryPolicy = GetRetryPolicy();
        }

        public async Task<IEnumerable<Email>> GetEmails()
        {
            var transferRequests = await _commitmentRepository.GetPendingTransferRequests();

            _logger.Info($"Found {transferRequests.Count} pending transfer requests");

            var distinctEmployerAccountIds = transferRequests.Select(x => x.SendingEmployerAccountId)
                .Distinct()
                .ToList();

            var distinctAccountsTasks =
                distinctEmployerAccountIds
                    .Select(x => _retryPolicy.ExecuteAndCaptureAsync(() => _accountApi.GetAccount(x)))
                    .ToList();

            var userPerAccountTasks =
                distinctEmployerAccountIds
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
                .Where(u => u.Users != null && accountIds.Contains(u.AccountId))
                .ToList();

            return transferRequests.SelectMany(transferRequest =>
            {
                var account = accounts.Single(a => a.AccountId == transferRequest.SendingEmployerAccountId);

                var users = accountsWithUsers.Where(a => a.AccountId == account.AccountId)
                    .SelectMany(a => a.Users)
                    .Where(u => u.CanReceiveNotifications
                                && (u.Role == "Owner" || u.Role == "Transactor"));

                return (users.Select(userModel =>
                    MapToEmail(userModel, transferRequest, account.HashedAccountId, account.DasAccountName)));
            });  
        }

        private Email MapToEmail(TeamMemberViewModel recipient, TransferRequestSummary transferRequest, string accountHashedAccountId, string accountDasAccountName)
        {
            return new Email
            {
                SystemId = "x",
                Subject ="x",
                ReplyToAddress = "digital.apprenticeship.service@notifications.service.gov.uk",
                TemplateId = "SendingEmployerTransferRequestNotification",
                RecipientsAddress = recipient.Email,
                Tokens = new Dictionary<string, string>
                {
                    {"cohort_reference", transferRequest.CohortReference},
                    {"receiver_name", transferRequest.ReceivingLegalEntityName},
                    {"transfers_dashboard_url", $"accounts/{accountHashedAccountId}/transfers"}
                }
            };
        }

        private async Task<UserModel> ToUserModel(long accountId)
        {
            var usersResult = await _retryPolicy.ExecuteAndCaptureAsync(() => _accountApi.GetAccountUsers(accountId));
            if (usersResult.Outcome == OutcomeType.Failure)
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

        private RetryPolicy GetRetryPolicy()
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
