using FluentAssertions;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.StopApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeship;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Exceptions;
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
        private GetApprenticeshipQueryResult _apprenticeQueryResult;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();

            _apprenticeQueryResult = _fixture.Build<GetApprenticeshipQueryResult>()
                .With(x => x.Status, ApprenticeshipStatus.Live).Create();
            _mediatorMock = new Mock<IMediator>();
            _mediatorMock.Setup(x => x.Send(It.IsAny<GetApprenticeshipQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(_apprenticeQueryResult);

            _loggerMock = new Mock<ILogger<AutomaticallyStopOverlappingTrainingDateRequestCommandHandler>>();
            _handler = new AutomaticallyStopOverlappingTrainingDateRequestCommandHandler(
                _mediatorMock.Object,
                _loggerMock.Object);
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
        public async Task Handle_DoesNotProcessIfNoApprenticeship()
        {
            // Arrange
            var message = _fixture.Create<AutomaticallyStopOverlappingTrainingDateRequestCommand>();
            _mediatorMock.Setup(x => x.Send(It.IsAny<GetApprenticeshipQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetApprenticeshipQueryResult) null);

            _mediatorMock.Setup(m => m.Send(It.IsAny<StopApprenticeshipCommand>(), It.IsAny<CancellationToken>())).ThrowsAsync(new BadRequestException("Apprenticeship 1 was not found"));

            // Act
            await _handler.Handle(message, Mock.Of<IMessageHandlerContext>());
        }

        [TestCase(ApprenticeshipStatus.Completed)]
        [TestCase(ApprenticeshipStatus.Stopped)]
        public async Task Handle_DoesNotProcessApprenticeshipWithThisStatus(ApprenticeshipStatus status)
        {
            // Arrange
            var message = _fixture.Create<AutomaticallyStopOverlappingTrainingDateRequestCommand>();
            _apprenticeQueryResult.Status = status;

            // Act
            await _handler.Handle(message, Mock.Of<IMessageHandlerContext>());
        }

        [Test]
        public void Handle_ThrowsAnUnexpectedException_LogsAndThrows()
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
