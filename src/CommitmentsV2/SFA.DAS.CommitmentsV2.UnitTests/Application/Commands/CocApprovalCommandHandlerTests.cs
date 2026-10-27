using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands;

[TestFixture]
public class CocApprovalCommandHandlerTests
{
    private Fixture _fixture;
    private Mock<ICocApprovalRulesEngine> _cocApprovalRules;
    private Mock<ILogger<CocApprovalCommandHandler>> _logger;
    private Mock<IMessageSession> _messageSession;
    private Mock<ICurrentDateTime> _currentDateTime;
    private ProviderCommitmentsDbContext _dbContext;
    private Lazy<ProviderCommitmentsDbContext> _lazyDbContext;
    private CocApprovalCommandHandler _sut;
    private readonly DateTime _utcNow = new(2026, 9, 7, 10, 0, 0, DateTimeKind.Utc);

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        var options = new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ProviderCommitmentsDbContext(options);
        _lazyDbContext = new Lazy<ProviderCommitmentsDbContext>(() => _dbContext);

        _cocApprovalRules = new Mock<ICocApprovalRulesEngine>();
        _logger = new Mock<ILogger<CocApprovalCommandHandler>>();
        _messageSession = new Mock<IMessageSession>();
        _currentDateTime = new Mock<ICurrentDateTime>();
        _currentDateTime.Setup(x => x.UtcNow).Returns(_utcNow);

        _sut = new CocApprovalCommandHandler(
            _lazyDbContext,
            _cocApprovalRules.Object,
            _logger.Object,
            _messageSession.Object,
            _currentDateTime.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }

    private CocApprovalItem CreateCocApprovalItem(Guid learningKey, CocApprovalItemStatus status = CocApprovalItemStatus.Pending, string field = "PlannedEndDate")
    {
        var item = _fixture.Build<CocApprovalItem>().With(f => f.LearningKey, learningKey).With(f => f.Status, status).With(x=>x.Field, field).Create();
        return item;
    }

    [Test]
    public async Task Handle_WhenCommandIsNull_ThrowsArgumentNullException()
    {
        // Act
        Func<Task> act = async () => await _sut.Handle(null, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("command");

        _cocApprovalRules.Verify(r => r.DetermineApprovalState(It.IsAny<CocApprovalDetails>()), Times.Never);
    }

    [Test]
    public async Task Handle_WhenActionIsCancelPrevious_AndPreviousApprovalItemExists_MarkAsCancelledAndReturnEmptyList()
    {
        // Arrange
        var learningKey = Guid.NewGuid();
        var existing = CreateCocApprovalItem(learningKey);
        await _dbContext.CocApprovalItems.AddAsync(existing);
        await _dbContext.SaveChangesAsync();

        var newChange = new List<CocApprovalFieldChange> { new CocApprovalFieldChange { ChangeType = "PlannedEndDate", Data = new CocData { Old = "2022-01-01", New = "2023-01-01" } } };

        var details = _fixture.Build<CocApprovalDetails>()
                .With(x => x.LearningKey, learningKey)
                .Without(x => x.Apprenticeship)
                .With(x => x.ApprovalFieldChanges, newChange)
                .Create();

        var command = new CocApprovalCommand
        {
            Action = AggregationAction.CancelPrevious,
            LearningKey = learningKey,
            CocApprovalDetails = details
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(CocApprovalResultStatus.Cancelled);
        result.Items.Should().NotBeNull().And.BeEmpty();

        var updatedItem = _dbContext.CocApprovalItems.Where(x => x.LearningKey == learningKey).FirstOrDefault();
        updatedItem.Status.Should().Be(CocApprovalItemStatus.Cancelled);

        _cocApprovalRules.Verify(r => r.DetermineApprovalState(It.IsAny<CocApprovalDetails>()), Times.Never);
        _messageSession.Verify(x => x.Send(It.IsAny<StoreLearningHistoryCommand>(), It.IsAny<SendOptions>()), Times.Never);
    }


    [Test]
    public async Task Handle_WhenActionIsSupersedePrevious_AndPreviousApprovalItemExists_MarksItSupersededAndReturnsRulesEngineResult()
    {
        // Arrange
        var learningKey = Guid.NewGuid();
        var existing = CreateCocApprovalItem(learningKey);
        await _dbContext.CocApprovalItems.AddAsync(existing);
        await _dbContext.SaveChangesAsync();

        var details = _fixture.Build<CocApprovalDetails>().With(x=>x.LearningKey, learningKey).Without(x=>x.Apprenticeship).With(x=>x.ApprovalFieldChanges, new List<CocApprovalFieldChange>()).Create();
        var item = new CocApprovalFieldChange() { ChangeType = "PlannedEndDate", Data = new CocData { New = "2026-01-01", Old = "2026=02-01" } };
        details.ApprovalFieldChanges.Add(item);

        var command = new CocApprovalCommand
        {
            Action = AggregationAction.SupersedePrevious,
            LearningKey = learningKey,
            CocApprovalDetails = details
        };

        var newApprovalRequest = new ApprovalRequest { Id = Guid.NewGuid(), Items = new List<ApprovalFieldRequest>() };
        var expectedResult = new CocApprovalResult { Status = CocApprovalResultStatus.Pending, Items = new List<CocUpdateResult>() { new CocUpdateResult { Field = CocChangeField.PlannedEndDate, Status = CocApprovalItemStatus.Pending } } };
        var state = new CocApprovalState { ApprovalRequest = newApprovalRequest, ApprovalResult = expectedResult };

        _cocApprovalRules
            .Setup(r => r.DetermineApprovalState(details))
            .ReturnsAsync(state);

        var beforeCall = DateTime.UtcNow;

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(expectedResult);

        existing.Status.Should().Be(CocApprovalItemStatus.Superseded);
        existing.Updated.Should().NotBeNull();
        existing.Updated!.Value.Should().BeOnOrAfter(beforeCall);

        var updatedItem = _dbContext.CocApprovalItems.Where(x => x.LearningKey == learningKey).FirstOrDefault();
        updatedItem.Status.Should().Be(CocApprovalItemStatus.Superseded);

        _cocApprovalRules.Verify(r => r.DetermineApprovalState(details), Times.Once);
    }

    [Test]
    public async Task Handle_WhenActionIsCreateNew_CallsRulesEngine_AddsReturnedApprovalRequest_AndReturnsResult()
    {
        // Arrange
        var details = new CocApprovalDetails { LearningKey = Guid.NewGuid() };
        var learningKey = Guid.NewGuid();
        var command = new CocApprovalCommand
        {
            Action = AggregationAction.CreateNew,
            LearningKey = learningKey,
            CocApprovalDetails = details
        };

        var newApprovalRequest = new ApprovalRequest { Id = Guid.NewGuid(),  Items = new List<ApprovalFieldRequest> { new ApprovalFieldRequest { Field = "PlannedEndDate", Old = "2026-01-02", New = "2026-02-01", Status = CocApprovalItemStatus.Pending } } };
        var expectedResult = new CocApprovalResult { Status = CocApprovalResultStatus.Pending, Items = new List<CocUpdateResult>() };
        var state = new CocApprovalState { ApprovalRequest = newApprovalRequest, ApprovalResult = expectedResult };

        _cocApprovalRules
            .Setup(r => r.DetermineApprovalState(details))
            .ReturnsAsync(state);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        await _dbContext.SaveChangesAsync();

        // Assert
        result.Should().BeSameAs(expectedResult);

        var addedItem = _dbContext.CocApprovalItems.Where(x => x.LearningKey == learningKey).FirstOrDefault();
        addedItem.Status.Should().Be(CocApprovalItemStatus.Pending);

        _cocApprovalRules.Verify(r => r.DetermineApprovalState(details), Times.Once);
        _messageSession.Verify(x => x.Send(It.IsAny<StoreLearningHistoryCommand>(), It.IsAny<SendOptions>()), Times.Never);
    }

    [Test]
    public async Task Handle_WhenActionIsCreateNew_AndItemsAreAutoRejected_SendsStoreLearningHistoryCommand()
    {
        var learningKey = Guid.NewGuid();
        var details = new CocApprovalDetails
        {
            LearningKey = learningKey,
            ApprenticeshipId = 12345,
            Updates = new CocUpdates
            {
                Firstname = new CocUpdate<string> { Old = "Test1", New = "Test2" },
                Lastname = new CocUpdate<string> { Old = "Last1", New = "Last2" }
            }
        };
        var command = new CocApprovalCommand
        {
            Action = AggregationAction.CreateNew,
            LearningKey = learningKey,
            CocApprovalDetails = details
        };

        var items = new List<ApprovalFieldRequest>
            {
                new() { Field = "Firstname", Old = "Test1", New = "Test2", Status = CocApprovalItemStatus.AutoRejected },
                new() { Field = "Lastname", Old = "Last1", New = "Last2", Status = CocApprovalItemStatus.AutoRejected }
            };
        var newApprovalRequest = new ApprovalRequest { Id = Guid.NewGuid(), Items = items };
        var expectedResult = new CocApprovalResult
        {
            Status = CocApprovalResultStatus.Complete,
            Items = new List<CocUpdateResult>
                {
                    new() { Field = CocChangeField.Firstname, Status = CocApprovalItemStatus.AutoRejected },
                    new() { Field = CocChangeField.Firstname, Status = CocApprovalItemStatus.AutoRejected }
                }
        };
        var state = new CocApprovalState { ApprovalRequest = newApprovalRequest, ApprovalResult = expectedResult };

        _cocApprovalRules
            .Setup(r => r.DetermineApprovalState(details))
            .ReturnsAsync(state);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().BeSameAs(expectedResult);

        var expectedDescription = "First name change from Test1 to Test2";
        _messageSession.Verify(x => x.Send(
            It.Is<StoreLearningHistoryCommand>(c =>
                c.ApprenticeshipId == details.ApprenticeshipId &&
                c.LearningKey == learningKey &&
                c.Source == LearningSourceType.ApprovalAPI &&
                c.ChangeType == LearningChangeType.AutoRejected &&
                c.AppliedDate == _utcNow &&
                c.Description == expectedDescription),
            It.IsAny<SendOptions>()), Times.Once);

        var anotherExpectedDescription = "Last name change from Last1 to Last2";
        _messageSession.Verify(x => x.Send(
            It.Is<StoreLearningHistoryCommand>(c =>
                c.ApprenticeshipId == details.ApprenticeshipId &&
                c.LearningKey == learningKey &&
                c.Source == LearningSourceType.ApprovalAPI &&
                c.ChangeType == LearningChangeType.AutoRejected &&
                c.AppliedDate == _utcNow &&
                c.Description == expectedDescription),
            It.IsAny<SendOptions>()), Times.Once);
    }

    [Test]
    public async Task Handle_WhenActionIsCreateNew_AndFirstnameIsAutoRejected_SendsAutoRejectedLearningHistory()
    {
        var details = new CocApprovalDetails
        {
            LearningKey = Guid.NewGuid(),
            ApprenticeshipId = 12345
        };

        var command = new CocApprovalCommand
        {
            Action = AggregationAction.CreateNew,
            CocApprovalDetails = details
        };

        var state = new CocApprovalState
        {
            ApprovalRequest = new ApprovalRequest
            {
                Id = Guid.NewGuid(),
                Items =
                [
                    new ApprovalFieldRequest
                    {
                        Field = nameof(CocChangeField.Firstname),
                        Old = "Bob",
                        New = "Bobby",
                        Status = CocApprovalItemStatus.AutoRejected
                    }
                ]
            },
            ApprovalResult = new CocApprovalResult()
        };

        _cocApprovalRules.Setup(x => x.DetermineApprovalState(details)).ReturnsAsync(state);

        await _sut.Handle(command, CancellationToken.None);

        _messageSession.Verify(x => x.Send(
                It.Is<StoreLearningHistoryCommand>(c =>
                    c.ApprenticeshipId == 12345 &&
                    c.LearningKey == details.LearningKey &&
                    c.Source == LearningSourceType.ApprovalAPI &&
                    c.ChangeType == LearningChangeType.AutoRejected &&
                    c.AppliedDate == _utcNow &&
                    c.Description == "First name change from Bob to Bobby"),
                It.IsAny<SendOptions>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_WhenActionIsCreateNew_AndFirstnameIsAutoApproved_SendsAutoApprovedLearningHistory()
    {
        var details = new CocApprovalDetails
        {
            LearningKey = Guid.NewGuid(),
            ApprenticeshipId = 12345
        };

        var command = new CocApprovalCommand
        {
            Action = AggregationAction.CreateNew,
            CocApprovalDetails = details
        };

        var state = new CocApprovalState
        {
            ApprovalRequest = new ApprovalRequest
            {
                Id = Guid.NewGuid(),
                Items =
                [
                    new ApprovalFieldRequest
                    {
                        Field = nameof(CocChangeField.Firstname),
                        Old = "Bob",
                        New = "Bobby",
                        Status = CocApprovalItemStatus.AutoApproved
                    }
                ]
            },
            ApprovalResult = new CocApprovalResult()
        };

        _cocApprovalRules.Setup(x => x.DetermineApprovalState(details)).ReturnsAsync(state);

        await _sut.Handle(command, CancellationToken.None);

        _messageSession.Verify(x => x.Send(
                It.Is<StoreLearningHistoryCommand>(c =>
                    c.ApprenticeshipId == 12345 &&
                    c.LearningKey == details.LearningKey &&
                    c.Source == LearningSourceType.ApprovalAPI &&
                    c.ChangeType == LearningChangeType.AutoApproved &&
                    c.AppliedDate == _utcNow &&
                    c.Description == "First name change from Bob to Bobby"),
                It.IsAny<SendOptions>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_WhenActionIsCreateNew_AndLastnameIsAutoRejected_SendsAutoRejectedLearningHistory()
    {
        var details = new CocApprovalDetails
        {
            LearningKey = Guid.NewGuid(),
            ApprenticeshipId = 12345
        };

        var command = new CocApprovalCommand
        {
            Action = AggregationAction.CreateNew,
            CocApprovalDetails = details
        };

        var state = new CocApprovalState
        {
            ApprovalRequest = new ApprovalRequest
            {
                Id = Guid.NewGuid(),
                Items =
                [
                    new ApprovalFieldRequest
                    {
                        Field = nameof(CocChangeField.Lastname),
                        Old = "Lewi",
                        New = "Lewis",
                        Status = CocApprovalItemStatus.AutoRejected
                    }
                ]
            },
            ApprovalResult = new CocApprovalResult()
        };

        _cocApprovalRules.Setup(x => x.DetermineApprovalState(details)).ReturnsAsync(state);

        await _sut.Handle(command, CancellationToken.None);

        _messageSession.Verify(x => x.Send(
                It.Is<StoreLearningHistoryCommand>(c =>
                    c.ApprenticeshipId == 12345 &&
                    c.LearningKey == details.LearningKey &&
                    c.Source == LearningSourceType.ApprovalAPI &&
                    c.ChangeType == LearningChangeType.AutoRejected &&
                    c.AppliedDate == _utcNow &&
                    c.Description == "Last name change from Lewi to Lewis"),
                It.IsAny<SendOptions>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_WhenActionIsCreateNew_AndLastnameIsAutoApproved_SendsAutoApprovedLearningHistory()
    {
        var details = new CocApprovalDetails
        {
            LearningKey = Guid.NewGuid(),
            ApprenticeshipId = 12345
        };

        var command = new CocApprovalCommand
        {
            Action = AggregationAction.CreateNew,
            CocApprovalDetails = details
        };

        var state = new CocApprovalState
        {
            ApprovalRequest = new ApprovalRequest
            {
                Id = Guid.NewGuid(),
                Items =
                [
                    new ApprovalFieldRequest
                    {
                        Field = nameof(CocChangeField.Lastname),
                        Old = "Lewi",
                        New = "Lewis",
                        Status = CocApprovalItemStatus.AutoApproved
                    }
                ]
            },
            ApprovalResult = new CocApprovalResult()
        };

        _cocApprovalRules.Setup(x => x.DetermineApprovalState(details)).ReturnsAsync(state);

        await _sut.Handle(command, CancellationToken.None);

        _messageSession.Verify(x => x.Send(
                It.Is<StoreLearningHistoryCommand>(c =>
                    c.ApprenticeshipId == 12345 &&
                    c.LearningKey == details.LearningKey &&
                    c.Source == LearningSourceType.ApprovalAPI &&
                    c.ChangeType == LearningChangeType.AutoApproved &&
                    c.AppliedDate == _utcNow &&
                    c.Description == "Last name change from Lewi to Lewis"),
                It.IsAny<SendOptions>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_WhenActionIsCreateNew_AndPlannedEndDateIsAutoRejected_SendsAutoRejectedLearningHistory()
    {
        var details = new CocApprovalDetails
        {
            LearningKey = Guid.NewGuid(),
            ApprenticeshipId = 12345
        };

        var command = new CocApprovalCommand
        {
            Action = AggregationAction.CreateNew,
            CocApprovalDetails = details
        };

        var state = new CocApprovalState
        {
            ApprovalRequest = new ApprovalRequest
            {
                Id = Guid.NewGuid(),
                Items =
                [
                    new ApprovalFieldRequest
                    {
                        Field = nameof(CocChangeField.PlannedEndDate),
                        Old = "2026-09-25",
                        New = "2026-10-01",
                        Status = CocApprovalItemStatus.AutoRejected
                    }
                ]
            },
            ApprovalResult = new CocApprovalResult()
        };

        _cocApprovalRules.Setup(x => x.DetermineApprovalState(details)).ReturnsAsync(state);

        await _sut.Handle(command, CancellationToken.None);

        _messageSession.Verify(x => x.Send(
                It.Is<StoreLearningHistoryCommand>(c =>
                    c.ApprenticeshipId == 12345 &&
                    c.LearningKey == details.LearningKey &&
                    c.Source == LearningSourceType.ApprovalAPI &&
                    c.ChangeType == LearningChangeType.AutoRejected &&
                    c.AppliedDate == _utcNow &&
                    c.Description == "Planned end date change from 2026-09-25 to 2026-10-01"),
                It.IsAny<SendOptions>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_WhenActionIsCreateNew_AndPlannedEndDateIsAutoApproved_SendsAutoApprovedLearningHistory()
    {
        var details = new CocApprovalDetails
        {
            LearningKey = Guid.NewGuid(),
            ApprenticeshipId = 12345
        };

        var command = new CocApprovalCommand
        {
            Action = AggregationAction.CreateNew,
            CocApprovalDetails = details
        };

        var state = new CocApprovalState
        {
            ApprovalRequest = new ApprovalRequest
            {
                Id = Guid.NewGuid(),
                Items =
                [
                    new ApprovalFieldRequest
                    {
                        Field = nameof(CocChangeField.PlannedEndDate),
                        Old = "2026-09-25",
                        New = "2026-10-01",
                        Status = CocApprovalItemStatus.AutoApproved
                    }
                ]
            },
            ApprovalResult = new CocApprovalResult()
        };

        _cocApprovalRules.Setup(x => x.DetermineApprovalState(details)).ReturnsAsync(state);

        await _sut.Handle(command, CancellationToken.None);

        _messageSession.Verify(x => x.Send(
                It.Is<StoreLearningHistoryCommand>(c =>
                    c.ApprenticeshipId == 12345 &&
                    c.LearningKey == details.LearningKey &&
                    c.Source == LearningSourceType.ApprovalAPI &&
                    c.ChangeType == LearningChangeType.AutoApproved &&
                    c.AppliedDate == _utcNow &&
                    c.Description == "Planned end date change from 2026-09-25 to 2026-10-01"),
                It.IsAny<SendOptions>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_WhenActionIsCreateNew_AndFieldOutsideEnumValuesIsAutoApproved_SendsAutoApprovedLearningHistory()
    {
        var details = new CocApprovalDetails
        {
            LearningKey = Guid.NewGuid(),
            ApprenticeshipId = 12345
        };

        var command = new CocApprovalCommand
        {
            Action = AggregationAction.CreateNew,
            CocApprovalDetails = details
        };

        var state = new CocApprovalState
        {
            ApprovalRequest = new ApprovalRequest
            {
                Id = Guid.NewGuid(),
                Items =
                [
                    new ApprovalFieldRequest
                    {
                        Field = "Random",
                        Old = "Value1",
                        New = "Value2",
                        Status = CocApprovalItemStatus.AutoApproved
                    }
                ]
            },
            ApprovalResult = new CocApprovalResult()
        };

        _cocApprovalRules.Setup(x => x.DetermineApprovalState(details)).ReturnsAsync(state);

        await _sut.Handle(command, CancellationToken.None);

        _messageSession.Verify(x => x.Send(
                It.Is<StoreLearningHistoryCommand>(c =>
                    c.ApprenticeshipId == 12345 &&
                    c.LearningKey == details.LearningKey &&
                    c.Source == LearningSourceType.ApprovalAPI &&
                    c.ChangeType == LearningChangeType.AutoApproved &&
                    c.AppliedDate == _utcNow &&
                    c.Description == "Random change from Value1 to Value2"),
                It.IsAny<SendOptions>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_WhenActionIsCreateNew_AndItemsAreAutoApproved_SendsStoreLearningHistoryCommand()
    {
        var details = new CocApprovalDetails { LearningKey = Guid.NewGuid(), ApprenticeshipId = 12345 };
        var command = new CocApprovalCommand
        {
            Action = AggregationAction.CreateNew,
            CocApprovalDetails = details
        };

        var items = new List<ApprovalFieldRequest>
        {
            new()
            {
                Field = nameof(CocChangeField.Firstname),
                Old = "Bob",
                New = "Bobby",
                Status = CocApprovalItemStatus.AutoApproved
            }
        };
        var state = new CocApprovalState
        {
            ApprovalRequest = new ApprovalRequest { Id = Guid.NewGuid(), Items = items },
            ApprovalResult = new CocApprovalResult { Status = CocApprovalResultStatus.Complete, Items = new List<CocUpdateResult>() }
        };

        _cocApprovalRules
            .Setup(r => r.DetermineApprovalState(details))
            .ReturnsAsync(state);

        await _sut.Handle(command, CancellationToken.None);

        _messageSession.Verify(x => x.Send(It.IsAny<StoreLearningHistoryCommand>(), It.IsAny<SendOptions>()), Times.Once);
    }

    [Test]
    public async Task Handle_WhenApprovalRequestItemsAreNull_DoesNotSendLearningHistory()
    {
        var details = new CocApprovalDetails
        {
            LearningKey = Guid.NewGuid()
        };

        var state = new CocApprovalState
        {
            ApprovalRequest = new ApprovalRequest
            {
                Id = Guid.NewGuid(),
                Items = null
            },
            ApprovalResult = new CocApprovalResult()
        };

        _cocApprovalRules.Setup(x => x.DetermineApprovalState(details)).ReturnsAsync(state);

        var command = new CocApprovalCommand
        {
            Action = AggregationAction.CreateNew,
            CocApprovalDetails = details
        };

        await _sut.Handle(command, CancellationToken.None);

        _messageSession.Verify(x => x.Send(It.IsAny<StoreLearningHistoryCommand>(), It.IsAny<SendOptions>()), Times.Never);
    }
}
