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

            _sut = new Application.Services.EventUpgradeHandler(_mockEndpointInstance.Object);
        }

        [Test]
        public async Task ThenTheEventIsPassedToTheEndpointInstance()
        {
            // arrange
            var testMessage = new Events.CohortApprovalRequestedByProvider(1, 2, 3);

            // act
            await _sut.Execute(testMessage);

            // assert
            _mockEndpointInstance.Verify(m => m.Publish(testMessage, It.IsAny<PublishOptions>()), Times.Once);
        }
    }
}
