using FluentAssertions;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.StopApprenticeship;
using SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.CommandHandlers
{
    [TestFixture]
    public class AutomaticallyStopOverlappingTrainingDateRequestCommandHandlerTests
    {
        private Mock<IMediator> _mediatorMock;
        private Mock<ILogger<AutomaticallyStopOverlappingTrainingDateRequestCommandHandler>> _loggerMock;
        private AutomaticallyStopOverlappingTrainingDateRequestCommandHandler _handler;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _mediatorMock = new Mock<IMediator>();
            _loggerMock = new Mock<ILogger<AutomaticallyStopOverlappingTrainingDateRequestCommandHandler>>();
            _handler = new AutomaticallyStopOverlappingTrainingDateRequestCommandHandler(
                _mediatorMock.Object,
                _loggerMock.Object);
            _fixture = new Fixture();
        }

        [Test]
        public async Task Handle_SendsStopApprenticeshipCommand()
        {
            // Arrange
            var message = _fixture.Create<AutomaticallyStopOverlappingTrainingDateRequestCommand>();

            // Act
            await _handler.Handle(message, Mock.Of<IMessageHandlerContext>());

            // Assert
            _mediatorMock.Verify(m => m.Send(
                It.Is<StopApprenticeshipCommand>(c =>
                    c.AccountId == message.AccountId 
                   && c.ApprenticeshipId == message.ApprenticeshipId
                   && c.StopDate == message.StopDate 
                   && c.MadeRedundant == false 
                   && c.UserInfo.IsSystem()
                   && c.Party == Types.Party.Employer
                    ), It.IsAny<CancellationToken>())
                    , Times.Once);
        }

        [Test]
        public void Handle_ThrowsException_LogsAndThrows()
        {
            // Arrange
            var message = _fixture.Create<AutomaticallyStopOverlappingTrainingDateRequestCommand>();
            var exception = new Exception("Test exception");
            _mediatorMock.Setup(m => m.Send(It.IsAny<StopApprenticeshipCommand>(), It.IsAny<CancellationToken>())).ThrowsAsync(exception);

            // Act & Assert
            Func<Task> act = async () => await _handler.Handle(message, Mock.Of<IMessageHandlerContext>());

            act.Should().ThrowAsync<Exception>().WithMessage("Handling AutomaticallyStopOverlappingTrainingDateRequestCommand failed");
        }
    }
}
