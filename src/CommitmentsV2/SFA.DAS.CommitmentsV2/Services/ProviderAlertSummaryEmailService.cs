using Microsoft.Extensions.Logging;
using NServiceBus;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq.Expressions;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class ProviderAlertSummaryEmailService : IProviderAlertSummaryEmailService
    {
        private readonly IProviderCommitmentsDbContext _context;
        private readonly ILogger<ProviderAlertSummaryEmailService> _logger;
        private readonly IMessageSession _nserviceBusContext;

        public ProviderAlertSummaryEmailService(IProviderCommitmentsDbContext context,
            ILogger<ProviderAlertSummaryEmailService> logger,
            IMessageSession nserviceBusContext)
        {
            _context = context;
            _logger = logger;
            _nserviceBusContext = nserviceBusContext;
        }

        public async Task SendAlertSummaryEmails(string jobId)
        {
            var alertSummaries = await GetProviderApprenticeshipAlertSummary();

            _logger.LogInformation($"Found {alertSummaries.Count} provider summary records.");

            if (alertSummaries.Count == 0)
                return;

            var distinctProviderIds = alertSummaries
                    .Select(m => m.ProviderId)
                    .Distinct()
                    .ToList();

            var stopwatch = Stopwatch.StartNew();
            _logger.LogDebug($"About to send emails to {distinctProviderIds.Count} providers, JobId: {jobId}");


            var sendEmailTasks = distinctProviderIds.Select(x => SendEmails(x, alertSummaries));
            await Task.WhenAll(sendEmailTasks);

            _logger.LogDebug($"Took {stopwatch.ElapsedMilliseconds} milliseconds to send {distinctProviderIds.Count} emails, JobId; {jobId}",
                new Dictionary<string, object>
                {
                    { "providerCount", distinctProviderIds.Count },
                    { "duration", stopwatch.ElapsedMilliseconds },
                    { "JobId", jobId }
                });
        }

        private async Task SendEmails(long providerId, IList<ProviderAlertSummary> alertSummaries)
        {
            var alert = alertSummaries.First(m => m.ProviderId == providerId);

            var sendEmailToProviderCommand = new SendEmailToProviderCommand(
                providerId,
                "ProviderAlertSummaryNotification2",
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
                    });

            await _nserviceBusContext.Send(sendEmailToProviderCommand);
        }

        private string GetMismatchText(int dataLockCount)
        {
            if (dataLockCount == 0)
                return string.Empty;

            if (dataLockCount == 1)
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

        private async Task<List<ProviderAlertSummary>> GetProviderApprenticeshipAlertSummary()
        {
            var summaries = new List<ProviderAlertSummary>();

            var providerSummaryInfos = await _context.Apprenticeships
                            .Include(app => app.Cohort)
                            .Include(app => app.Cohort.Provider)
                            .Where(app => app.PaymentStatus > 0)
                            .Where(app => app.PendingUpdateOriginator != null ||
                            app.DataLockStatus.AsQueryable().Any(unhandledCourseOrPriceDlock))
                            .Select(app => new
                            {
                                ProviderId = app.Cohort.ProviderId,
                                ProviderName = app.Cohort.Provider.Name,
                                PendingOriginator = app.PendingUpdateOriginator,
                                DLocks = app.DataLockStatus
                            }).ToListAsync();

            var providerGroups = providerSummaryInfos.GroupBy(app => app.ProviderId);

            foreach (var providerGroup in providerGroups)
            {
                summaries.Add(new ProviderAlertSummary
                    {
                        ProviderId = providerGroup.First().ProviderId,
                        ProviderName = providerGroup.First().ProviderName,
                        TotalCount = providerGroup.Count(),
                        ChangesForReview = providerGroup.Count(app => app.PendingOriginator == 0),
                        DataMismatchCount = providerGroup.Count(app => app.DLocks.Any(
                            dlock => DataLockStatusExtensions.UnHandled(dlock)
                                    && (DataLockStatusExtensions.WithCourseError(dlock)
                                        || DataLockStatusExtensions.IsPriceOnly(dlock))))
                    });
            }

            return summaries;
        }

        Expression<Func<SFA.DAS.CommitmentsV2.Models.DataLockStatus, bool>> unhandledCourseOrPriceDlock = dl =>
       !dl.IsResolved && dl.Status != Types.Status.Pass && !dl.IsExpired
       && dl.ErrorCode.HasFlag(DataLockErrorCode.Dlock03)
               || dl.ErrorCode.HasFlag(DataLockErrorCode.Dlock04)
               || dl.ErrorCode.HasFlag(DataLockErrorCode.Dlock05)
               || dl.ErrorCode.HasFlag(DataLockErrorCode.Dlock06)
               || dl.ErrorCode.HasFlag(DataLockErrorCode.Dlock07);

    }
}