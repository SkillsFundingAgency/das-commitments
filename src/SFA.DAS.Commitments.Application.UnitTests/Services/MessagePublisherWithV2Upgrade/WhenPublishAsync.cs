using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Messaging.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Services.MessagePublisherWithV2Upgrade
{
    [TestFixture]
    public class WhenPublishAsync
    {
        private Application.Services.MessagePublisherWithV2Upgrade _sut;
        private Mock<IMessagePublisher> _mockMessagePublisher;
        private Mock<IEventConsumer> _mockEventConsumer;

        [SetUp]
        public void Arrange()
        {
            _mockMessagePublisher = new Mock<IMessagePublisher>();
            _mockEventConsumer = new Mock<IEventConsumer>();
            
            _sut = new Application.Services.MessagePublisherWithV2Upgrade(_mockMessagePublisher.Object, _mockEventConsumer.Object);
        }

        [Test]
        public async Task ThenTheEventIsPassedToTheMessagePublisher()
        {
            // arrange
            var testMessage = new { };

            // act
            await _sut.PublishAsync(testMessage);

            // assert
            _mockMessagePublisher.Verify(m => m.PublishAsync(testMessage), Times.Once);
        }

        [Test]
        public async Task ThenTheEventIsPassedToTheEventConsumer()
        {
            // arrange
            var testMessage = new { } as object;

            // act
            await _sut.PublishAsync(testMessage);

            // assert
            _mockEventConsumer.Verify(m => m.Consume(testMessage), Times.Once);
        }
    }
}
