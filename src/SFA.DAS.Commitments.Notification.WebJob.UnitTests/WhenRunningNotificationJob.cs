using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;

using SFA.DAS.Commitments.Notification.WebJob.Configuration;
using SFA.DAS.Commitments.Notification.WebJob.EmailServices;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Notifications.Api.Client;
using SFA.DAS.Notifications.Api.Types;

namespace SFA.DAS.Commitments.Notification.WebJob.UnitTests
{
    [TestFixture]
    public class WhenRunningNotificationJob
    {
        private Mock<IEmployerAlertSummaryEmailService> _mockEmailService;
        private Mock<INotificationsApi> _mockNotificationApi;
        private NotificationJob _sur;
        private Mock<IProviderAlertSummaryEmailService> _providerEmailService;
        private Mock<ISendingEmployerTransferRequestEmailService> _sendingEmployerTransferRequestEmailService;

        [SetUp]
        public void SetUp()
        {
            _mockEmailService = new Mock<IEmployerAlertSummaryEmailService>();
            _mockNotificationApi = new Mock<INotificationsApi>();
            _providerEmailService = new Mock<IProviderAlertSummaryEmailService>();
            _sendingEmployerTransferRequestEmailService = new Mock<ISendingEmployerTransferRequestEmailService>();

            _sur = new NotificationJob(
                _mockEmailService.Object, 
                _providerEmailService.Object,
                _sendingEmployerTransferRequestEmailService.Object,
                _mockNotificationApi.Object, 
                Mock.Of<ILog>(),
                new CommitmentNotificationConfiguration {SendEmail = true});
        }

        [Test]
        public async Task ShouldCallNotificationForEachEmail()
        {
            var fixture = new Fixture();
            var emails = fixture.CreateMany<Email>(5);
            _mockEmailService.Setup(m => m.GetEmails()).ReturnsAsync(emails);
            await _sur.RunEmployerAlertSummaryNotification("JobId");

            _mockEmailService.Verify(m => m.GetEmails(), Times.Once);
            _mockNotificationApi.Verify(m => m.SendEmail(It.IsAny<Email>()), Times.Exactly(5));
        }

        [Test]
        public async Task ShouldCallProviderAlertEmailServiceToSendEmails()
        {
            await _sur.RunProviderAlertSummaryNotification("JobId");

            _providerEmailService.Verify(m => m.SendAlertSummaryEmails("JobId"), Times.Once);
        }

        [Test]
        public async Task ShouldNotCallProviderAlertEmailServiceToSendEmailsWhenConfigSendEmailIsFalse()
        {
            _sur = new NotificationJob(
                _mockEmailService.Object,
                _providerEmailService.Object,
                _sendingEmployerTransferRequestEmailService.Object,
                _mockNotificationApi.Object,
                Mock.Of<ILog>(),
                new CommitmentNotificationConfiguration { SendEmail = false });

            await _sur.RunProviderAlertSummaryNotification("JobId");

            _providerEmailService.Verify(m => m.SendAlertSummaryEmails(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task ShouldCallEmployerNotificationForEachTransferRequestEmail()
        {
            var fixture = new Fixture();
            var emails = fixture.CreateMany<Email>(3);
            _sendingEmployerTransferRequestEmailService.Setup(m => m.GetEmails()).ReturnsAsync(emails);
            await _sur.RunSendingEmployerTransferRequestNotification("JobId");

            _sendingEmployerTransferRequestEmailService.Verify(m => m.GetEmails(), Times.Once);
            _mockNotificationApi.Verify(m => m.SendEmail(It.IsAny<Email>()), Times.Exactly(3));
        }
    }
}