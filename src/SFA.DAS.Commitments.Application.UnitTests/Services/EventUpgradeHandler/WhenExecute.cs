using System.Threading.Tasks;
using Moq;
using NServiceBus;
using NUnit.Framework;

namespace SFA.DAS.Commitments.Application.UnitTests.Services.EventUpgradeHandler
{
    [TestFixture]
    public class WhenExecute
    {
        private Application.Services.EventUpgradeHandler _sut;
        private Mock<IEndpointInstance> _mockEndpointInstance;
        private Mock<NLog.Logger.ILog> _mockLogger;

        [SetUp]
        public void Arrange()
        {
            _mockEndpointInstance = new Mock<IEndpointInstance>();
            _mockLogger = new Mock<NLog.Logger.ILog>();
            _mockLogger.Setup(m => m.Debug(It.IsAny<string>())).Verifiable();

            _sut = new Application.Services.EventUpgradeHandler(_mockEndpointInstance.Object, _mockLogger.Object);
        }

        [Test]
        public async Task ThenTheCohortApprovalRequestedByProviderEventIsPassedToTheEndpointInstance()
        {
            // arrange
            var testMessage = new Events.CohortApprovalRequestedByProvider(1, 2, 3);

            // act
            await _sut.Execute(testMessage);

            // assert
            _mockEndpointInstance.Verify(m => m.Publish(
                It.Is<CommitmentsV2.Messages.Events.CohortApprovalRequestedByProviderEvent>(a => 
                a.AccountId.Equals(testMessage.AccountId) && 
                a.ProviderId.Equals(testMessage.ProviderId) &&
                a.CommitmentId.Equals(testMessage.CommitmentId))
                , It.IsAny<PublishOptions>())
                , Times.Once);
        }

        [Test]
        public async Task ThenTheCohortApprovalRequestedByProviderEventHandlerLogsTheEventUpgrade()
        {
            // arrange
            var testMessage = new Events.CohortApprovalRequestedByProvider(1, 2, 3);

            // act
            await _sut.Execute(testMessage);

            // assert
            _mockLogger.Verify(m => m.Debug($"Upgrading {nameof(Events.CohortApprovalRequestedByProvider)} to publish with NServiceBus"), Times.Once);
        }

        [Test]
        public async Task ThenTheCohortApprovedByEmployerEventIsPassedToTheEndpointInstance()
        {
            // arrange
            var testMessage = new Events.CohortApprovedByEmployer(1, 2, 3);

            // act
            await _sut.Execute(testMessage);

            // assert
            _mockEndpointInstance.Verify(m => m.Publish(
                It.Is<CommitmentsV2.Messages.Events.CohortApprovedByEmployerEvent>(a =>
                a.AccountId.Equals(testMessage.AccountId) &&
                a.ProviderId.Equals(testMessage.ProviderId) &&
                a.CommitmentId.Equals(testMessage.CommitmentId))
                , It.IsAny<PublishOptions>())
                , Times.Once);
        }

        [Test]
        public async Task ThenTheCohortApprovedByEmployerEventHandlerLogsTheEventUpgrade()
        {
            // arrange
            var testMessage = new Events.CohortApprovedByEmployer(1, 2, 3);

            // act
            await _sut.Execute(testMessage);

            // assert
            _mockLogger.Verify(m => m.Debug($"Upgrading {nameof(Events.CohortApprovedByEmployer)} to publish with NServiceBus"), Times.Once);
        }
    }
}
