using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Extensions;
using SFA.DAS.NLog.Logger;
using SFA.DAS.PAS.Account.Api.Client;
using SFA.DAS.PAS.Account.Api.Types;

namespace SFA.DAS.Commitments.Notification.WebJob.EmailServices
{
    public class ProviderAlertSummaryEmailService : IProviderAlertSummaryEmailService
    {
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly ILog _logger;
        private readonly IPasAccountApiClient _providerAccountClient;
        private readonly Policy _retryPolicy;

        // set at 50 to limit the number of concurrent api calls to stay below the S0 azure database user limit
        private const int MaxConcurrentThreads = 50;

        public ProviderAlertSummaryEmailService(
            IApprenticeshipRepository apprenticeshipRepository,
            IPasAccountApiClient providerAccountClient,
            ILog logger)
        {
            _apprenticeshipRepository = apprenticeshipRepository;
            _logger = logger;
            _providerAccountClient = providerAccountClient;
            _retryPolicy = GetRetryPolicy();
        }

        public async Task SendAlertSummaryEmails(string jobId)
        {
            var alertSummaries = await _apprenticeshipRepository.GetProviderApprenticeshipAlertSummary();

            _logger.Info($"Found {alertSummaries.Count} provider summary records.");

            if (alertSummaries.Count == 0)
            {
                return;
            }

            var distinctProviderIds = alertSummaries
                    .Select(m => m.ProviderId)
                    .Distinct()
                    .ToList();

            var stopwatch = Stopwatch.StartNew();
            _logger.Debug($"About to send emails to {distinctProviderIds.Count} providers, JobId: {jobId}");

            var taskQueue = new ConcurrentQueue<Task>(distinctProviderIds
                .Select(x => _retryPolicy.ExecuteAndCaptureAsync(() => SendEmails(x, alertSummaries))));
            
            var taskRunners = new List<Task>();
            for (int n = 0; n < Math.Min(distinctProviderIds.Count, MaxConcurrentThreads); n++)
            {
                taskRunners.Add(Task.Run(async () => {
                    while (taskQueue.TryDequeue(out Task sendEmailsTask))
                    {
                        await sendEmailsTask;
                    }
                }));
            }

            await Task.WhenAll(taskRunners);

            _logger.Debug($"Took {stopwatch.ElapsedMilliseconds} milliseconds to send {distinctProviderIds.Count} emails, JobId; {jobId}",
                new Dictionary<string, object>
                {
                    { "providerCount", distinctProviderIds.Count },
                    { "duration", stopwatch.ElapsedMilliseconds },
                    { "JobId", jobId }
                });
        }

        private Task SendEmails(long providerId, IList<ProviderAlertSummary> alertSummaries)
        {
            var alert = alertSummaries.First(m => m.ProviderId == providerId);

            var email = new ProviderEmailRequest
            {
                TemplateId = "ProviderAlertSummaryNotification2",
                Tokens =
                    new Dictionary<string, string>
                    {
                        {"total_count_text", alert.TotalCount.ToString()},
                        {"need_needs", alert.TotalCount > 1 ? "need" : "needs"},
                        {"changes_for_review", ChangesForReviewText(alert.ChangesForReview)},
                        {"mismatch_changes", GetMismatchText(alert.DataMismatchCount)},
                        {
                            "link_to_mange_apprenticeships",
                            $"{providerId}/apprentices/manage/all?RecordStatus=ChangesForReview&RecordStatus=IlrDataMismatch&RecordStatus=ChangeRequested"
                        }
                    }
            };
            return _providerAccountClient.SendEmailToAllProviderRecipients(providerId, email);
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

        private RetryPolicy GetRetryPolicy()
        {
            return Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(3, retryAttempt =>
                    {
                        _logger.Warn($"Error connecting to PAS Account Api: Retrying...attempt {retryAttempt})");
                        return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                    }
                );
        }
    }
}