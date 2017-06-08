using System.Threading.Tasks;

using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture;

using SFA.DAS.NLog.Logger;
using SFA.DAS.Notifications.Api.Client;
using SFA.DAS.Notifications.Api.Types;

namespace SFA.DAS.Commitments.Notification.WebJob.UnitTests
{
    [TestFixture]
    public class WhenRunningNotificationJob
    {
        private Mock<IEmailTemplatesService> _mockEmailService;
        private Mock<INotificationsApi> _mockNotificationApi;
        private NotificationJob _sur;

        [SetUp]
        public void SetUp()
        {
            _mockEmailService = new Mock<IEmailTemplatesService>();
            _mockNotificationApi = new Mock<INotificationsApi>();
            _sur = new NotificationJob(_mockEmailService.Object, _mockNotificationApi.Object, Mock.Of<ILog>());
        }

        [Test]
        public async Task ShouldCallNotificationForEachEmail()
        {
            var fixture = new Fixture();
            var emails = fixture.CreateMany<Email>(5);
            _mockEmailService.Setup(m => m.GetEmails()).ReturnsAsync(emails);
            await _sur.Run();

            _mockEmailService.Verify(m => m.GetEmails(), Times.Once);
            _mockNotificationApi.Verify(m => m.SendEmail(It.IsAny<Email>()), Times.Exactly(5));
        }
    }
}