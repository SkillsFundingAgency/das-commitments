using Microsoft.Extensions.Logging;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.CommitmentsV2.Application.Commands.AcceptApprenticeshipUpdates;
using SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeship;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    public class ApprenticeshipStartDateChangedEventHandlerTests
    {
        private readonly Fixture _fixture;
        private Mock<ILogger<ApprenticeshipStartDateChangedEventHandler>> _mockLogger;
        private Mock<IMediator> _mockMediator;
        private Mock<IMessageHandlerContext> _mockIMessageHandlerContext;

        public ApprenticeshipStartDateChangedEventHandlerTests()
        {
            _fixture = new Fixture();
        }

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<ApprenticeshipStartDateChangedEventHandler>>();
            _mockMediator = new Mock<IMediator>();
            _mockIMessageHandlerContext = new Mock<IMessageHandlerContext>();
        }

        [Test]
        public void Handle_FailsToResolveUsers_ThrowsException()
        {
            //Arrange
            var handler = new ApprenticeshipStartDateChangedEventHandler(_mockLogger.Object, _mockMediator.Object);
            var message = _fixture.Create<ApprenticeshipStartDateChangedEvent>();

            //Act
            var actual = Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(message, _mockIMessageHandlerContext.Object));

            //Assert
            Assert.That(actual.Message, Is.EqualTo($"Invalid initiator {message.Initiator}"));
        }

        [Test]
        public void Handle_EditApprenticeshipCommandFails_ThrowsException()
        {
            //Arrange
            var handler = new ApprenticeshipStartDateChangedEventHandler(_mockLogger.Object, _mockMediator.Object);
            var message = _fixture.Create<ApprenticeshipStartDateChangedEvent>();
            message.Initiator = "Provider";

            _mockMediator.Setup(x => x.Send(It.IsAny<EditApprenticeshipCommand>(), default)).ThrowsAsync(new Exception("TEST"));

            //Act
            var actual = Assert.ThrowsAsync<Exception>(() => handler.Handle(message, _mockIMessageHandlerContext.Object));

            //Assert
            Assert.That(actual.Message, Is.EqualTo("TEST"));
        }

        [Test]
        public void Handle_ApproveApprenticeshipFails_ThrowsException()
        {
            //Arrange
            var handler = new ApprenticeshipStartDateChangedEventHandler(_mockLogger.Object, _mockMediator.Object);
            var message = _fixture.Create<ApprenticeshipStartDateChangedEvent>();
            message.Initiator = "Provider";

            _mockMediator.Setup(x => x.Send(It.IsAny<AcceptApprenticeshipUpdatesCommand>(), default)).ThrowsAsync(new Exception("TEST"));

            //Act
            var actual = Assert.ThrowsAsync<Exception>(() => handler.Handle(message, _mockIMessageHandlerContext.Object));

            //Assert
            Assert.That(actual.Message, Is.EqualTo("TEST"));
        }


        [Test]
        public void Handle_CompletesSuccessfully()
        {
            //Arrange
            var handler = new ApprenticeshipStartDateChangedEventHandler(_mockLogger.Object, _mockMediator.Object);
            var message = _fixture.Create<ApprenticeshipStartDateChangedEvent>();
            message.Initiator = "Provider";

            //Act / Assert
            Assert.DoesNotThrowAsync(() => handler.Handle(message, _mockIMessageHandlerContext.Object));

        }

        [Test]
        public async Task Handle_SetsDatesCorrectly()
        {
	        //Arrange
	        var handler = new ApprenticeshipStartDateChangedEventHandler(_mockLogger.Object, _mockMediator.Object);
	        var message = _fixture.Create<ApprenticeshipStartDateChangedEvent>();
	        message.Initiator = "Provider";

	        //Act
	        await handler.Handle(message, _mockIMessageHandlerContext.Object);

		    // Assert
			_mockMediator.Verify(x => x.Send(It.Is<EditApprenticeshipCommand>(command 
	            => command.EditApprenticeshipRequest.ActualStartDate == message.ActualStartDate
	               && command.EditApprenticeshipRequest.StartDate.Value.Year == message.ActualStartDate.Year
				   && command.EditApprenticeshipRequest.StartDate.Value.Month == message.ActualStartDate.Month
	               && command.EditApprenticeshipRequest.StartDate.Value.Day == 1
			), It.IsAny<CancellationToken>()));

        }
	}
}