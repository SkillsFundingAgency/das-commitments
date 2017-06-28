using System.Threading.Tasks;

using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture;

using SFA.DAS.Commitments.Notification.WebJob.Configuration;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Notifications.Api.Client;
using SFA.DAS.Notifications.Api.Types;

namespace SFA.DAS.Commitments.Notification.WebJob.UnitTests
{
    [TestFixture]
    public class WhenRunningNotificationJob
    {
        private Mock<IEmployerEmailTemplatesService> _mockEmailService;
        private Mock<INotificationsApi> _mockNotificationApi;
        private NotificationJob _sur;
        private Mock<IProviderEmailTemplatesService> _providerEmailService;

        [SetUp]
        public void SetUp()
        {
            _mockEmailService = new Mock<IEmployerEmailTemplatesService>();
            _mockNotificationApi = new Mock<INotificationsApi>();
            _providerEmailService = new Mock<IProviderEmailTemplatesService>();
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