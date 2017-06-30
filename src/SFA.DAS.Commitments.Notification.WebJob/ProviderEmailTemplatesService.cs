using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Infrastructure.Data;
using SFA.DAS.Notifications.Api.Types;

namespace SFA.DAS.Commitments.Notification.WebJob
{
    public class ProviderEmailTemplatesService : IProviderEmailTemplatesService
    {
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly ICommitmentsLogger _logger;
        private readonly IProviderEmailServiceWrapper   _emailService;

        public ProviderEmailTemplatesService(
            IApprenticeshipRepository apprenticeshipRepository,
            ICommitmentsLogger logger,
            IProviderEmailServiceWrapper emailService)
        {
            _apprenticeshipRepository = apprenticeshipRepository;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<IEnumerable<Email>> GetEmails()
        {
            var alertSummaries = await _apprenticeshipRepository.GetProviderApprenticeshipAlertSummary();

            _logger.Info($"Found {alertSummaries.Count} summary records.");

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

            var emails = providers.SelectMany(
                p => p.Select(m => MapToEmail(m, alertSummaries)));

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
            var  users = await _emailService.GetUsersAsync(ukprn);
            if (users == null)
            {
                _logger.Warn($"Can't find and user for provider {ukprn}.");
                return new List<ProviderUser>();
            }

            users.ForEach(m => m.Ukprn = ukprn);
            return users;
        }
    }
}