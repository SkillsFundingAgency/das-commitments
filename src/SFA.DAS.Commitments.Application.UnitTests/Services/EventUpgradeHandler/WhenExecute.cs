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

        [SetUp]
        public void Arrange()
        {
            _mockEndpointInstance = new Mock<IEndpointInstance>();

            _sut = new Application.Services.EventUpgradeHandler(_mockEndpointInstance.Object, Mock.Of<NLog.Logger.ILog>());
        }

        [Test]
        public async Task ThenTheEventIsPassedToTheEndpointInstance()
        {
            // arrange
            var testMessage = new Events.CohortApprovalRequestedByProvider(1, 2, 3);

            // act
            await _sut.Execute(testMessage);

            // assert
            _mockEndpointInstance.Verify(m => m.Publish(
                It.Is<CommitmentsV2.Messages.Events.CohortApprovalRequestedByProvider>(a => 
                a.AccountId.Equals(testMessage.AccountId) && 
                a.ProviderId.Equals(testMessage.ProviderId) &&
                a.CommitmentId.Equals(testMessage.CommitmentId))
                , It.IsAny<PublishOptions>())
                , Times.Once);
        }
    }
}
