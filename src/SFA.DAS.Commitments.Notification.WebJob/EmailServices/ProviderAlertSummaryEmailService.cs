using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Infrastructure.Data;
using SFA.DAS.Notifications.Api.Types;
using SFA.DAS.PAS.Account.Api.Client;

namespace SFA.DAS.Commitments.Notification.WebJob.EmailServices
{
    public class ProviderAlertSummaryEmailService : IProviderAlertSummaryEmailService
    {
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly ICommitmentsLogger _logger;
        private readonly IPasAccountApiClient _providerAccountClient;
        private RetryPolicy _retryPolicy;

        public ProviderAlertSummaryEmailService(
            IApprenticeshipRepository apprenticeshipRepository,
            ICommitmentsLogger logger,
            IPasAccountApiClient providerAccountClient)
        {
            _apprenticeshipRepository = apprenticeshipRepository;
            _logger = logger;
            _providerAccountClient = providerAccountClient;
            _retryPolicy = GetRetryPolicy();
        }

        public async Task<IEnumerable<Email>> GetEmails()
        {
            var alertSummaries = await _apprenticeshipRepository.GetProviderApprenticeshipAlertSummary();

            _logger.Info($"Found {alertSummaries.Count} provider summary records.");

            var distinctProviderIds =
                alertSummaries
                .Select(m => m.ProviderId)
                .Distinct()
                .ToList();

            var getProviderUsersTasks =
                distinctProviderIds
                    .Select(GetNormalUsersForProvider)
                .ToList();

            await Task.WhenAll(getProviderUsersTasks);
            var providers = getProviderUsersTasks.Select(x => x.Result).ToList();

            var emails = providers
                .SelectMany(p => p
                     .Select(m => MapToEmail(m, alertSummaries)));

            return emails;
        }

        private Email MapToEmail(ProviderUserInfo user, IList<ProviderAlertSummary> alertSummaries)
        {
            var alert = alertSummaries.Single(m => m.ProviderId == user.ProviderId);
            return new Email
            {
                RecipientsAddress = user.Email,
                TemplateId = "ProviderAlertSummaryNotification",
                ReplyToAddress = "digital.apprenticeship.service@notifications.service.gov.uk",
                Subject = "Items for your attention: apprenticeship service",
                SystemId = "x",
                Tokens =
                    new Dictionary<string, string>
                        {
                            { "name", user.Name },
                            { "total_count_text", alert.TotalCount == 1
                                ? "is 1 apprentice"
                                : $"are {alert.TotalCount} apprentices" },
                            { "provider_name", alert.ProviderName },
                            { "need_needs", alert.TotalCount > 1 ? "need" :"needs" },
                            { "changes_for_review", ChangesForReviewText(alert.ChangesForReview) },
                            { "mismatch_changes", GetMismatchText(alert.DataMismatchCount) },
                            { "link_to_mange_apprenticeships", $"{user.ProviderId}/apprentices/manage/all?RecordStatus=ChangesForReview&RecordStatus=IlrDataMismatch&RecordStatus=ChangeRequested" }
                        }
            };
        }

        private string GetMismatchText(int dataLockCount)
        {
            if (dataLockCount == 0)
                return string.Empty;

            if(dataLockCount == 1)
                return "* 1 apprentice with an ILR data mismatch";

            return $"* {dataLockCount} apprentices with an ILR data mismatch";
        }

        private string ChangesForReviewText(int changesForReview)
        {
            if (changesForReview == 0)
                return string.Empty;

            if (changesForReview == 1)
                return "* 1 apprentice with changes for review";

            return $"* {changesForReview} apprentices with changes for review";
        }

        private async Task<IEnumerable<ProviderUserInfo>> GetNormalUsersForProvider(long ukprn)
        {
            var accountUserResult = (await _retryPolicy.ExecuteAndCaptureAsync(() => _providerAccountClient.GetAccountUsers(ukprn)));
            var accountUsers = accountUserResult.Result?.Where(u=>!u.IsSuperUser && u.ReceiveNotifications).ToArray();

            return accountUsers.Select(u=> new ProviderUserInfo { Email = u.EmailAddress, Name = GetFirstName(u.DisplayName), ProviderId = ukprn});
        }

        private string GetFirstName(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                return "";
            }

            var names = displayName.Split(' ');

            return names[0];
        }

        private RetryPolicy GetRetryPolicy()
        {
            return Policy
                    .Handle<Exception>()
                    .RetryAsync(3,
                        (exception, retryCount) =>
                        {
                            _logger.Warn($"Error connecting to PAS Account Api: ({exception.Message}). Retrying...attempt {retryCount})");
                        }
                    );
        }
    }

    class ProviderUserInfo
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public long ProviderId { get; set; }
    }
}