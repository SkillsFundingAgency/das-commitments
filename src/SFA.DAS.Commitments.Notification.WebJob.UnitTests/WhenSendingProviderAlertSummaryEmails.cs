using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Notification.WebJob.EmailServices;
using SFA.DAS.NLog.Logger;
using SFA.DAS.PAS.Account.Api.Client;
using SFA.DAS.PAS.Account.Api.Types;

namespace SFA.DAS.Commitments.Notification.WebJob.UnitTests
{
    [TestFixture]
    public class WhenSendingProviderAlertSummaryEmails
    {
        [Test]
        public async Task AndNoSummariesFound_ThenShouldNotCallSendEmailToAllProviderRecipients()
        {
            var f = new WhenSendingProviderAlertSummaryEmailsFixture();
            await f.Sut.SendAlertSummaryEmails(f.JobId);
            f.VerifySendEmailToAllProviderRecipientsIsNeverCalled();
        }

        [Test]
        public async Task AndOneSummaryFound_ThenShouldCallSendEmailToAllProviderRecipientsOnce()
        {
            var f = new WhenSendingProviderAlertSummaryEmailsFixture().WithOneSummaryAlert();
            await f.Sut.SendAlertSummaryEmails(f.JobId);
            f.VerifySendEmailToAllProviderRecipientsIsCalledOnceWithSummaryAlert(f.FirstAlertSummary);
        }

        [Test]
        public async Task AndTwoDuplicateSummariesFound_ThenShouldCallSendEmailToAllProviderRecipientsOnce()
        {
            var f = new WhenSendingProviderAlertSummaryEmailsFixture().WithDuplicateSummaryAlert();
            await f.Sut.SendAlertSummaryEmails(f.JobId);
            f.VerifySendEmailToAllProviderRecipientsIsCalledOnceWithSummaryAlert(f.FirstAlertSummary);
        }

        [Test]
        public async Task AndDifferentSummariesFound_ThenShouldCallSendEmailToAllProviderRecipientsOnceForEachProvider()
        {
            var f = new WhenSendingProviderAlertSummaryEmailsFixture().WithMultipleSummaryAlerts();
            await f.Sut.SendAlertSummaryEmails(f.JobId);
            f.VerifySendEmailToAllProviderRecipientsIsCalledOnceWithSummaryAlertForEachProvider();
        }

        [Test]
        public async Task AndOneSummaryFoundAndFirstAndSecondCallToApiFails_ThenShouldCallSendEmailToAllProviderRecipientsUsingRetryPolicy()
        {
            var f = new WhenSendingProviderAlertSummaryEmailsFixture().WithOneSummaryAlert().WithFirstAndSecondApiCallsFailure();
            await f.Sut.SendAlertSummaryEmails(f.JobId);
            f.VerifySendEmailToAllProviderRecipientsIsCalledNTimeWithSummaryAlert(f.FirstAlertSummary, 3);
        }
    }

    public class WhenSendingProviderAlertSummaryEmailsFixture
    {
        public Mock<IApprenticeshipRepository> ApprenticeshipRepository;
        public ProviderAlertSummaryEmailService Sut;
        public Mock<IPasAccountApiClient> PasAccountApiClient;
        public string JobId;
        public ProviderAlertSummary FirstAlertSummary;
        public ProviderAlertSummary SecondAlertSummary;
        public ProviderAlertSummary AlertSummaryNoApprenticeChanges;
        public ProviderAlertSummary AlertSummaryNoDataLocks;

        public WhenSendingProviderAlertSummaryEmailsFixture()
        {
            var fixture = new Fixture();

            JobId = fixture.Create<string>();
            FirstAlertSummary = fixture.Create<ProviderAlertSummary>();
            SecondAlertSummary = fixture.Create<ProviderAlertSummary>();
            AlertSummaryNoApprenticeChanges = fixture.Build<ProviderAlertSummary>().With(x => x.ChangesForReview, 0).Create();
            AlertSummaryNoDataLocks = fixture.Build<ProviderAlertSummary>().With(x => x.DataMismatchCount, 0).Create();

            ApprenticeshipRepository = new Mock<IApprenticeshipRepository>();
            ApprenticeshipRepository.Setup(x => x.GetProviderApprenticeshipAlertSummary())
                .ReturnsAsync(new List<ProviderAlertSummary>());

            PasAccountApiClient = new Mock<IPasAccountApiClient>();
            Sut = new ProviderAlertSummaryEmailService(ApprenticeshipRepository.Object, PasAccountApiClient.Object, Mock.Of<ILog>());
        }

        internal void VerifySendEmailToAllProviderRecipientsIsNeverCalled()
        {
            PasAccountApiClient.Verify(x=>x.SendEmailToAllProviderRecipients(It.IsAny<long>(), It.IsAny<ProviderEmailRequest>()), Times.Never);
        }

        internal WhenSendingProviderAlertSummaryEmailsFixture WithOneSummaryAlert()
        {
            ApprenticeshipRepository.Setup(x=>x.GetProviderApprenticeshipAlertSummary()).ReturnsAsync(new List<ProviderAlertSummary>
            {
                FirstAlertSummary
            });
            return this;
        }

        internal WhenSendingProviderAlertSummaryEmailsFixture WithDuplicateSummaryAlert()
        {
            ApprenticeshipRepository.Setup(x => x.GetProviderApprenticeshipAlertSummary()).ReturnsAsync(new List<ProviderAlertSummary>
            {
                FirstAlertSummary,
                FirstAlertSummary
            });
            return this;
        }

        internal WhenSendingProviderAlertSummaryEmailsFixture WithMultipleSummaryAlerts()
        {
            ApprenticeshipRepository.Setup(x => x.GetProviderApprenticeshipAlertSummary()).ReturnsAsync(new List<ProviderAlertSummary>
            {
                FirstAlertSummary,
                SecondAlertSummary,
                FirstAlertSummary,
                SecondAlertSummary,
                AlertSummaryNoApprenticeChanges,
                AlertSummaryNoDataLocks
            });
            return this;
        }

        internal WhenSendingProviderAlertSummaryEmailsFixture WithFirstAndSecondApiCallsFailure()
        {
            int count = 0;
            PasAccountApiClient.Setup(x =>x.SendEmailToAllProviderRecipients(It.IsAny<long>(), It.IsAny<ProviderEmailRequest>()))
                .Callback( () =>
                    {
                        count++;
                        if(count <= 2)
                            throw new HttpRequestException();
                    });
            return this;
        }


        internal void VerifySendEmailToAllProviderRecipientsIsCalledOnceWithSummaryAlertForEachProvider()
        {
            VerifySendEmailToAllProviderRecipientsIsCalledOnceWithSummaryAlert(FirstAlertSummary);
            VerifySendEmailToAllProviderRecipientsIsCalledOnceWithSummaryAlert(SecondAlertSummary);
            VerifySendEmailToAllProviderRecipientsIsCalledOnceWithSummaryAlert(AlertSummaryNoApprenticeChanges);
            VerifySendEmailToAllProviderRecipientsIsCalledOnceWithSummaryAlert(AlertSummaryNoDataLocks);
        }

        internal void VerifySendEmailToAllProviderRecipientsIsCalledOnceWithSummaryAlert(ProviderAlertSummary alertSummary)
        {
            VerifySendEmailToAllProviderRecipientsIsCalledNTimeWithSummaryAlert(alertSummary, 1);
        }

        internal void VerifySendEmailToAllProviderRecipientsIsCalledNTimeWithSummaryAlert(ProviderAlertSummary alertSummary, int n)
        {
            PasAccountApiClient.Verify(x => x.SendEmailToAllProviderRecipients(alertSummary.ProviderId,
                It.Is<ProviderEmailRequest>(p => p.TemplateId == "ProviderAlertSummaryNotification2" && ValidateTokens(p.Tokens, alertSummary))), Times.Exactly(n));
        }

        private bool ValidateTokens(Dictionary<string, string> tokens, ProviderAlertSummary alertSummary)
        {
            return tokens["total_count_text"] == alertSummary.TotalCount.ToString() 
                    && tokens["link_to_mange_apprenticeships"].StartsWith(alertSummary.ProviderId.ToString())
                    && alertSummary.ChangesForReview == 0 ? 
                            string.IsNullOrWhiteSpace(tokens["changes_for_review"]) :
                            tokens["changes_for_review"].StartsWith("* " + alertSummary.ChangesForReview)
                    && alertSummary.DataMismatchCount == 0 ? 
                            string.IsNullOrWhiteSpace(tokens["mismatch_changes"]) :
                            tokens["mismatch_changes"].StartsWith("* " + alertSummary.DataMismatchCount);
        }

    }
}
