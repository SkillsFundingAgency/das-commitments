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
        private Mock<IEmployerAlertSummaryEmailTemplateService> _mockEmailService;
        private Mock<INotificationsApi> _mockNotificationApi;
        private NotificationJob _sur;
        private Mock<IProviderAlertSummaryEmailTemplateService> _providerEmailService;

        [SetUp]
        public void SetUp()
        {
            _mockEmailService = new Mock<IEmployerAlertSummaryEmailTemplateService>();
            _mockNotificationApi = new Mock<INotificationsApi>();
            _providerEmailService = new Mock<IProviderAlertSummaryEmailTemplateService>();
            _sur = new NotificationJob(
                _mockEmailService.Object, 
                _providerEmailService.Object, 
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
            await _sur.RunEmployerNotification("JobId");

            _mockEmailService.Verify(m => m.GetEmails(), Times.Once);
            _mockNotificationApi.Verify(m => m.SendEmail(It.IsAny<Email>()), Times.Exactly(5));
        }

        [Test]
        public async Task ShouldCallProviderNotificationForEachEmail()
        {
            var fixture = new Fixture();
            var emails = fixture.CreateMany<Email>(3);
            _providerEmailService.Setup(m => m.GetEmails()).ReturnsAsync(emails);
            await _sur.RunProviderNotification("JobId");

            _providerEmailService.Verify(m => m.GetEmails(), Times.Once);
            _mockNotificationApi.Verify(m => m.SendEmail(It.IsAny<Email>()), Times.Exactly(3));
        }
    }
}