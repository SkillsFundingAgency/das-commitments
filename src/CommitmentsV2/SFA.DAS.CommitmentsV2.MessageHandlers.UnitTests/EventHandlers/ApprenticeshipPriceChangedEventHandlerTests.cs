using FluentAssertions;
using Microsoft.Extensions.Logging;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Apprenticeships.Types.Models;
using SFA.DAS.CommitmentsV2.Application.Commands.AcceptApprenticeshipUpdates;
using SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeship;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers;

[TestFixture]
public class ApprenticeshipPriceChangedEventHandlerTests
{
    private readonly Fixture _fixture;
    private Mock<ILogger<ApprenticeshipPriceChangedEventHandler>> _mockLogger;
    private Mock<IMediator> _mockMediator;
    private Mock<IMessageHandlerContext> _mockIMessageHandlerContext;

    public ApprenticeshipPriceChangedEventHandlerTests()
    {
        _fixture = new Fixture();
    }

    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<ApprenticeshipPriceChangedEventHandler>>();
        _mockMediator = new Mock<IMediator>();
        _mockIMessageHandlerContext = new Mock<IMessageHandlerContext>();
    }

    [Test]
    public void Handle_FailsToResolveUsers_ThrowsException()
    {
        //Arrange
        var handler = new ApprenticeshipPriceChangedEventHandler(_mockLogger.Object, _mockMediator.Object);
        var message = _fixture.Create<ApprenticeshipPriceChangedEvent>();
        message.ApprovedBy = (Apprenticeships.Types.Enums.ApprovedBy)99;//invalid value

        //Act
        var actual = Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(message, _mockIMessageHandlerContext.Object));

        //Assert
        actual.Message.Should().Be($"Invalid initiator {message.ApprovedBy}");
    }

    [Test]
    public void Handle_EditApprenticeshipCommandFails_ThrowsException()
    {
        //Arrange
        var handler = new ApprenticeshipPriceChangedEventHandler(_mockLogger.Object, _mockMediator.Object);
        var message = _fixture.Create<ApprenticeshipPriceChangedEvent>();
        message.ApprovedBy = Apprenticeships.Types.Enums.ApprovedBy.Employer;

        _mockMediator.Setup(x => x.Send(It.IsAny<EditApprenticeshipCommand>(), default)).ThrowsAsync(new Exception("TEST"));

        //Act
        var actual = Assert.ThrowsAsync<Exception>(() => handler.Handle(message, _mockIMessageHandlerContext.Object));

        //Assert
        actual.Message.Should().Be("TEST");
    }

    [Test]
    public void Handle_ApproveApprenticeshipFails_ThrowsException()
    {
        //Arrange
        var handler = new ApprenticeshipPriceChangedEventHandler(_mockLogger.Object, _mockMediator.Object);
        var message = _fixture.Create<ApprenticeshipPriceChangedEvent>();
        message.ApprovedBy = Apprenticeships.Types.Enums.ApprovedBy.Employer;

        _mockMediator.Setup(x => x.Send(It.IsAny<AcceptApprenticeshipUpdatesCommand>(), default)).ThrowsAsync(new Exception("TEST"));

        //Act
        var actual = Assert.ThrowsAsync<Exception>(() => handler.Handle(message, _mockIMessageHandlerContext.Object));

        //Assert
        actual.Message.Should().Be("TEST");
    }


    [Test]
    public void Handle_CompletesSuccessfully()
    {
        //Arrange
        var handler = new ApprenticeshipPriceChangedEventHandler(_mockLogger.Object, _mockMediator.Object);
        var message = _fixture.Create<ApprenticeshipPriceChangedEvent>();
        message.ApprovedBy = Apprenticeships.Types.Enums.ApprovedBy.Employer;

        //Act / Assert
        Action act = () => handler.Handle(message, _mockIMessageHandlerContext.Object);
        act.Should().NotThrow();
    }

    [Test]
    public async Task Handle_SetsPricesCorrectly()
    {
        //Arrange
        var handler = new ApprenticeshipPriceChangedEventHandler(_mockLogger.Object, _mockMediator.Object);
        var message = _fixture.Create<ApprenticeshipPriceChangedEvent>();
        message.ApprovedBy = Apprenticeships.Types.Enums.ApprovedBy.Employer;
        message.Episode.Prices = new List<ApprenticeshipEpisodePrice>
        {
            new() { TrainingPrice = 7000, EndPointAssessmentPrice = 500 },
        };

        //Act
        await handler.Handle(message, _mockIMessageHandlerContext.Object);

        // Assert
        _mockMediator.Verify(x => x.Send(It.Is<EditApprenticeshipCommand>(command
            => command.EditApprenticeshipRequest.Cost == 7500
               && command.EditApprenticeshipRequest.TrainingPrice == 7000
               && command.EditApprenticeshipRequest.EndPointAssessmentPrice == 500
        ), It.IsAny<CancellationToken>()));
    }
}