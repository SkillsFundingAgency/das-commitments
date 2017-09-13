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

namespace SFA.DAS.Commitments.Notification.WebJob
{
    public class ProviderEmailTemplatesService : IProviderEmailTemplatesService
    {
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly ICommitmentsLogger _logger;
        private readonly IProviderEmailServiceWrapper   _emailService;
        private readonly IAccountApiClient _providerAccountClient;
        private RetryPolicy _retryPolicy;

        public ProviderEmailTemplatesService(
            IApprenticeshipRepository apprenticeshipRepository,
            ICommitmentsLogger logger,
            IProviderEmailServiceWrapper emailService,
            IAccountApiClient providerAccountClient)
        {
            _apprenticeshipRepository = apprenticeshipRepository;
            _logger = logger;
            _emailService = emailService;
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

            var providerTasks =
                distinctProviderIds
                .Select(GetProvider)
                .ToList();

            await Task.WhenAll(providerTasks);
            var providers = providerTasks.Select(x => x.Result).ToList();

            var emails = providers
                .SelectMany(p => p
                     .Where(m => m.ReceiveNotifications)
                     .Select(m => MapToEmail(m, alertSummaries)));

            return emails;
        }

        private Email MapToEmail(ProviderUser user, IList<ProviderAlertSummary> alertSummaries)
        {
            var alert = alertSummaries.Single(m => m.ProviderId == user.Ukprn);
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
                            { "name", user.GivenName },
                            { "total_count_text", alert.TotalCount == 1
                                ? "is 1 apprentice"
                                : $"are {alert.TotalCount} apprentices" },
                            { "provider_name", alert.ProviderName },
                            { "changes_for_review", alert.ChangesForReview > 0
                                ? $"* {alert.ChangesForReview} with changes for review"
                                : string.Empty },
                            { "mismatch_changes", alert.DataMismatchCount > 0
                                ? $"* {alert.DataMismatchCount} with an ILR data mismatch"
                                : string.Empty },
                            { "link_to_mange_apprenticeships", $"{user.Ukprn}/apprentices/manage/all?RecordStatus=ChangesForReview&RecordStatus=IlrDataMismatch&RecordStatus=ChangeRequested" }
                        }
            };
        }

        private async Task<IEnumerable<ProviderUser>> GetProvider(long ukprn)
        {
            var users = await _emailService.GetUsersAsync(ukprn);
            if (users == null)
            {
                _logger.Warn($"Can't find and user for provider {ukprn}.");
                return new List<ProviderUser>();
            }

            var accountUserResult = (await _retryPolicy.ExecuteAndCaptureAsync(() => _providerAccountClient.GetAccountUsers(ukprn)));
            var accountUser = accountUserResult.Result?.ToArray();

            foreach (var user in users)
            {
                var u = accountUser
                    ?.FirstOrDefault(m => user.Email.Trim().ToLower() == m.EmailAddress.Trim()
                    ?.ToLower());

                user.ReceiveNotifications = u == null || u.ReceiveNotifications;
                user.Ukprn = ukprn;
            }

            return users;
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
}