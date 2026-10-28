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
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands;

[TestFixture]
public class CocApprovalCommandHandlerTests
{
    private Fixture _fixture;
    private Mock<ICocApprovalRulesEngine> _cocApprovalRules;
    private Mock<ILogger<CocApprovalCommandHandler>> _logger;
    private Mock<IMessageSession> _messageSession;
    private Mock<ICurrentDateTime> _currentDateTime;
    private Mock<INotifyProviderService> _notifyProviderService;
    private Mock<IEncodingService> _encodingService;
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
        _notifyProviderService = new Mock<INotifyProviderService>();
        _encodingService = new Mock<IEncodingService>();
        _encodingService.Setup(x => x.Encode(It.IsAny<long>(), EncodingType.ApprenticeshipId)).Returns("hashed-apprenticeship");

        _sut = new CocApprovalCommandHandler(
            _lazyDbContext,
            _cocApprovalRules.Object,
            _logger.Object,
            _messageSession.Object,
            _currentDateTime.Object,
            _notifyProviderService.Object,
            _encodingService.Object);
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
        updatedItem.Status.Should().Be(CocApprovalItemStatus.Reversal);

        _cocApprovalRules.Verify(r => r.DetermineApprovalState(It.IsAny<CocApprovalDetails>()), Times.Never);
        _messageSession.Verify(x => x.Send(It.IsAny<StoreLearningHistoryCommand>(), It.IsAny<SendOptions>()), Times.Never);
    }

    [Test]
    public async Task Handle_WhenActionIsCancelPrevious_AndDetailsAreNull_DoesNotThrowAndLeavesThePendingRow()
    {
        var learningKey = Guid.NewGuid();
        var existing = CreateCocApprovalItem(learningKey);
        await _dbContext.CocApprovalItems.AddAsync(existing);
        await _dbContext.SaveChangesAsync();

        var result = await _sut.Handle(new CocApprovalCommand
        {
            Action = AggregationAction.CancelPrevious,
            LearningKey = learningKey,
            CocApprovalDetails = null
        }, CancellationToken.None);

        result.Items.Should().NotBeNull().And.BeEmpty();
        existing.Status.Should().Be(CocApprovalItemStatus.Pending);
        _cocApprovalRules.Verify(r => r.DetermineApprovalState(It.IsAny<CocApprovalDetails>()), Times.Never);
    }

    [Test]
    public async Task Handle_WhenOneFieldReversesAndAnotherIsSuperseded_ClosesOnlyTheRevertedField()
    {
        var learningKey = Guid.NewGuid();
        var firstName = CreateCocApprovalItem(learningKey, field: "Firstname");
        var lastName = CreateCocApprovalItem(learningKey, field: "Lastname");
        await _dbContext.CocApprovalItems.AddRangeAsync(firstName, lastName);
        await _dbContext.SaveChangesAsync();

        var details = _fixture.Build<CocApprovalDetails>()
            .With(x => x.LearningKey, learningKey)
            .Without(x => x.Apprenticeship)
            .With(x => x.ApprovalFieldChanges, new List<CocApprovalFieldChange>
            {
                new CocApprovalFieldChange { ChangeType = "Lastname", Data = new CocData { Old = "Smith", New = "Jones" } }
            })
            .Create();

        var expectedResult = new CocApprovalResult { Status = CocApprovalResultStatus.Pending, Items = new List<CocUpdateResult>() };
        _cocApprovalRules
            .Setup(r => r.DetermineApprovalState(details))
            .ReturnsAsync(new CocApprovalState
            {
                ApprovalRequest = new ApprovalRequest { Id = Guid.NewGuid(), Items = new List<ApprovalFieldRequest>() },
                ApprovalResult = expectedResult
            });

        await _sut.Handle(new CocApprovalCommand
        {
            Action = AggregationAction.SupersedePrevious,
            LearningKey = learningKey,
            CocApprovalDetails = details,
            ReversedFields = ["Firstname"]
        }, CancellationToken.None);

        firstName.Status.Should().Be(CocApprovalItemStatus.Reversal);
        lastName.Status.Should().Be(CocApprovalItemStatus.Superseded);
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

    [Test]
    public async Task Handle_WhenPricesAreOmitted_LeavesPendingPriceRow()
    {
        var learningKey = Guid.NewGuid();
        var price = CreateCocApprovalItem(learningKey, field: nameof(CocChangeField.Prices));
        var endDate = CreateCocApprovalItem(learningKey);
        await _dbContext.CocApprovalItems.AddRangeAsync(price, endDate);
        await _dbContext.SaveChangesAsync();

        var details = DetailsWithHistory(learningKey);
        details.ApprovalFieldChanges = [new CocApprovalFieldChange { ChangeType = nameof(CocChangeField.PlannedEndDate), Data = new CocData { Old = "2026-03-01", New = "2026-08-01" } }];
        SetupRules(details, new CocApprovalResult
        {
            Status = CocApprovalResultStatus.Pending,
            Items = [new CocUpdateResult { Field = CocChangeField.PlannedEndDate, Status = CocApprovalItemStatus.Pending }]
        });

        await _sut.Handle(new CocApprovalCommand
        {
            Action = AggregationAction.SupersedePrevious,
            LearningKey = learningKey,
            CocApprovalDetails = details
        }, CancellationToken.None);

        price.Status.Should().Be(CocApprovalItemStatus.Pending);
        endDate.Status.Should().Be(CocApprovalItemStatus.Superseded);
    }

    [Test]
    public async Task Handle_WhenLatestPriceMatchesHistory_MarksPendingPriceAsReversalAndDoesNotNotifyOrWriteHistory()
    {
        var learningKey = Guid.NewGuid();
        var price = CreateCocApprovalItem(learningKey, field: nameof(CocChangeField.Prices));
        await _dbContext.CocApprovalItems.AddAsync(price);
        await _dbContext.SaveChangesAsync();

        var details = DetailsWithHistory(learningKey);
        SetupRules(details, new CocApprovalResult { Status = CocApprovalResultStatus.Complete, Items = [] });

        var result = await _sut.Handle(new CocApprovalCommand
        {
            Action = AggregationAction.CreateNew,
            LearningKey = learningKey,
            CocApprovalDetails = details,
            Prices = [new PriceRecord { EffectiveFrom = new DateTime(2024, 8, 1), TrainingPrice = 1000, AssessmentPrice = 500 }]
        }, CancellationToken.None);

        price.Status.Should().Be(CocApprovalItemStatus.Reversal);
        _dbContext.CocApprovalItems.Count(item => item.Field == nameof(CocChangeField.Prices)).Should().Be(1);
        result.Items.Should().ContainSingle(item => item.Field == CocChangeField.Prices && item.Status == CocApprovalItemStatus.Reversal && item.Reason == null);
        _messageSession.Verify(x => x.Send(It.IsAny<StoreLearningHistoryCommand>(), It.IsAny<SendOptions>()), Times.Never);
        _notifyProviderService.Verify(x => x.NotifyProvider(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task Handle_WhenPriceDoesNotMatchHistory_SupersedesPendingPriceAndStoresPendingRow()
    {
        var learningKey = Guid.NewGuid();
        var price = CreateCocApprovalItem(learningKey, field: nameof(CocChangeField.Prices));
        await _dbContext.CocApprovalItems.AddAsync(price);
        await _dbContext.SaveChangesAsync();

        var details = DetailsWithHistory(learningKey);
        SetupRules(details, new CocApprovalResult { Status = CocApprovalResultStatus.Pending, Items = [] });

        var result = await _sut.Handle(new CocApprovalCommand
        {
            Action = AggregationAction.CreateNew,
            LearningKey = learningKey,
            CocApprovalDetails = details,
            Prices = [new PriceRecord { EffectiveFrom = new DateTime(2025, 9, 1), TrainingPrice = 1200, AssessmentPrice = 900 }]
        }, CancellationToken.None);

        await _dbContext.SaveChangesAsync();

        price.Status.Should().Be(CocApprovalItemStatus.Superseded);
        var pending = _dbContext.CocApprovalItems.Single(item => item.Field == nameof(CocChangeField.Prices) && item.Status == CocApprovalItemStatus.Pending);
        pending.Old.Should().Contain("1500");
        pending.New.Should().Contain("2100");
        result.Items.Should().ContainSingle(item => item.Field == CocChangeField.Prices && item.Status == CocApprovalItemStatus.Pending);
        _messageSession.Verify(x => x.Send(It.IsAny<StoreLearningHistoryCommand>(), It.IsAny<SendOptions>()), Times.Never);
    }

    [Test]
    public async Task Handle_WhenPriceContainsAFutureDate_SupersedesPendingPriceAndStoresAutoRejected()
    {
        var learningKey = Guid.NewGuid();
        var price = CreateCocApprovalItem(learningKey, field: nameof(CocChangeField.Prices));
        await _dbContext.CocApprovalItems.AddAsync(price);
        await _dbContext.SaveChangesAsync();

        var details = DetailsWithHistory(learningKey);
        SetupRules(details, new CocApprovalResult
        {
            Status = CocApprovalResultStatus.Pending,
            Items = [new CocUpdateResult { Field = CocChangeField.PlannedEndDate, Status = CocApprovalItemStatus.Pending }]
        });

        var result = await _sut.Handle(new CocApprovalCommand
        {
            Action = AggregationAction.CreateNew,
            LearningKey = learningKey,
            CocApprovalDetails = details,
            Prices =
            [
                new PriceRecord { EffectiveFrom = new DateTime(2025, 9, 1), TrainingPrice = 1200, AssessmentPrice = 900 },
                new PriceRecord { EffectiveFrom = new DateTime(2026, 11, 1), TrainingPrice = 1300, AssessmentPrice = 900 }
            ]
        }, CancellationToken.None);

        await _dbContext.SaveChangesAsync();

        price.Status.Should().Be(CocApprovalItemStatus.Superseded);
        _dbContext.CocApprovalItems.Should().NotContain(item => item.Field == nameof(CocChangeField.Prices) && item.Status == CocApprovalItemStatus.Pending);
        var rejected = _dbContext.CocApprovalItems.Single(item => item.Status == CocApprovalItemStatus.AutoRejected);
        rejected.Field.Should().Be(nameof(CocChangeField.Prices));
        rejected.Reason.Should().Be(PriceChangeDecider.FuturePriceReason);
        result.Items.Should().Contain(item => item.Field == CocChangeField.Prices && item.Status == CocApprovalItemStatus.AutoRejected && item.Reason == PriceChangeDecider.FuturePriceReason);
        result.Items.Should().Contain(item => item.Field == CocChangeField.PlannedEndDate && item.Status == CocApprovalItemStatus.Pending);
        _messageSession.Verify(x => x.Send(It.Is<StoreLearningHistoryCommand>(command => command.Description == PriceChangeDecider.FuturePriceReason), It.IsAny<SendOptions>()), Times.Once);
        _notifyProviderService.Verify(x => x.NotifyProvider(details.ProviderId, "hashed-apprenticeship", "ProviderRequestRejectedNotification", null), Times.Once);
    }

    [Test]
    public async Task Handle_WhenEndDateReverses_MarksOnlyThatRowAsReversal()
    {
        var learningKey = Guid.NewGuid();
        var endDate = CreateCocApprovalItem(learningKey);
        var price = CreateCocApprovalItem(learningKey, field: nameof(CocChangeField.Prices));
        await _dbContext.CocApprovalItems.AddRangeAsync(endDate, price);
        await _dbContext.SaveChangesAsync();

        var details = DetailsWithHistory(learningKey);
        SetupRules(details, new CocApprovalResult { Status = CocApprovalResultStatus.Complete, Items = [] });

        await _sut.Handle(new CocApprovalCommand
        {
            Action = AggregationAction.CreateNew,
            LearningKey = learningKey,
            CocApprovalDetails = details,
            ReversedFields = [nameof(CocChangeField.PlannedEndDate)]
        }, CancellationToken.None);

        endDate.Status.Should().Be(CocApprovalItemStatus.Reversal);
        price.Status.Should().Be(CocApprovalItemStatus.Pending);
        _messageSession.Verify(x => x.Send(It.IsAny<StoreLearningHistoryCommand>(), It.IsAny<SendOptions>()), Times.Never);
    }

    [Test]
    public async Task Handle_WhenPricesListIsEmpty_LeavesPendingPriceRow()
    {
        var learningKey = Guid.NewGuid();
        var price = CreateCocApprovalItem(learningKey, field: nameof(CocChangeField.Prices));
        await _dbContext.CocApprovalItems.AddAsync(price);
        await _dbContext.SaveChangesAsync();

        var details = DetailsWithHistory(learningKey);
        SetupRules(details, new CocApprovalResult { Status = CocApprovalResultStatus.Complete, Items = [] });

        await _sut.Handle(new CocApprovalCommand
        {
            Action = AggregationAction.CreateNew,
            LearningKey = learningKey,
            CocApprovalDetails = details,
            Prices = []
        }, CancellationToken.None);

        price.Status.Should().Be(CocApprovalItemStatus.Pending);
    }

    [Test]
    public async Task Handle_WhenPriceReversesBesideARealEndDateChange_MarksEachFieldOnItsOwn()
    {
        var learningKey = Guid.NewGuid();
        var price = CreateCocApprovalItem(learningKey, field: nameof(CocChangeField.Prices));
        var endDate = CreateCocApprovalItem(learningKey);
        await _dbContext.CocApprovalItems.AddRangeAsync(price, endDate);
        await _dbContext.SaveChangesAsync();

        var details = DetailsWithHistory(learningKey);
        details.ApprovalFieldChanges = [new CocApprovalFieldChange { ChangeType = nameof(CocChangeField.PlannedEndDate), Data = new CocData { Old = "2026-03-01", New = "2026-08-01" } }];
        SetupRules(details, new CocApprovalResult
        {
            Status = CocApprovalResultStatus.Pending,
            Items = [new CocUpdateResult { Field = CocChangeField.PlannedEndDate, Status = CocApprovalItemStatus.Pending }]
        });

        var result = await _sut.Handle(new CocApprovalCommand
        {
            Action = AggregationAction.SupersedePrevious,
            LearningKey = learningKey,
            CocApprovalDetails = details,
            Prices = [new PriceRecord { EffectiveFrom = new DateTime(2024, 8, 1), TrainingPrice = 1000, AssessmentPrice = 500 }]
        }, CancellationToken.None);

        price.Status.Should().Be(CocApprovalItemStatus.Reversal);
        endDate.Status.Should().Be(CocApprovalItemStatus.Superseded);
        result.Items.Should().Contain(item => item.Field == CocChangeField.Prices && item.Status == CocApprovalItemStatus.Reversal);
        result.Items.Should().Contain(item => item.Field == CocChangeField.PlannedEndDate && item.Status == CocApprovalItemStatus.Pending);
    }

    [Test]
    public async Task Handle_WhenEndDateReversesBesideARealPriceChange_MarksEachFieldOnItsOwn()
    {
        var learningKey = Guid.NewGuid();
        var price = CreateCocApprovalItem(learningKey, field: nameof(CocChangeField.Prices));
        var endDate = CreateCocApprovalItem(learningKey);
        await _dbContext.CocApprovalItems.AddRangeAsync(price, endDate);
        await _dbContext.SaveChangesAsync();

        var details = DetailsWithHistory(learningKey);
        SetupRules(details, new CocApprovalResult { Status = CocApprovalResultStatus.Pending, Items = [] });

        var result = await _sut.Handle(new CocApprovalCommand
        {
            Action = AggregationAction.CreateNew,
            LearningKey = learningKey,
            CocApprovalDetails = details,
            ReversedFields = [nameof(CocChangeField.PlannedEndDate)],
            Prices = [new PriceRecord { EffectiveFrom = new DateTime(2025, 9, 1), TrainingPrice = 1200, AssessmentPrice = 900 }]
        }, CancellationToken.None);

        await _dbContext.SaveChangesAsync();

        endDate.Status.Should().Be(CocApprovalItemStatus.Reversal);
        price.Status.Should().Be(CocApprovalItemStatus.Superseded);
        _dbContext.CocApprovalItems.Should().Contain(item => item.Field == nameof(CocChangeField.Prices) && item.Status == CocApprovalItemStatus.Pending);
        result.Items.Should().Contain(item => item.Field == CocChangeField.Prices && item.Status == CocApprovalItemStatus.Pending);
        _messageSession.Verify(x => x.Send(It.IsAny<StoreLearningHistoryCommand>(), It.IsAny<SendOptions>()), Times.Never);
    }

    [Test]
    public async Task Handle_WhenAnotherFieldIsAlreadyAutoRejected_DoesNotSendASecondProviderEmail()
    {
        var learningKey = Guid.NewGuid();
        var details = DetailsWithHistory(learningKey);
        SetupRules(details, new CocApprovalResult
        {
            Status = CocApprovalResultStatus.Complete,
            Items = [new CocUpdateResult { Field = CocChangeField.PlannedEndDate, Status = CocApprovalItemStatus.AutoRejected, Reason = "The end date must not be before the start date" }]
        });

        await _sut.Handle(new CocApprovalCommand
        {
            Action = AggregationAction.CreateNew,
            LearningKey = learningKey,
            CocApprovalDetails = details,
            Prices = [new PriceRecord { EffectiveFrom = new DateTime(2026, 11, 1), TrainingPrice = 1300, AssessmentPrice = 900 }]
        }, CancellationToken.None);

        await _dbContext.SaveChangesAsync();

        _dbContext.CocApprovalItems.Should().ContainSingle(item => item.Status == CocApprovalItemStatus.AutoRejected && item.Reason == PriceChangeDecider.FuturePriceReason);
        _notifyProviderService.Verify(x => x.NotifyProvider(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _messageSession.Verify(x => x.Send(It.Is<StoreLearningHistoryCommand>(command => command.Description == PriceChangeDecider.FuturePriceReason), It.IsAny<SendOptions>()), Times.Once);
    }

    private CocApprovalDetails DetailsWithHistory(Guid learningKey)
    {
        return new CocApprovalDetails
        {
            LearningKey = learningKey,
            ApprenticeshipId = 44021,
            ProviderId = 100,
            ULN = "1234567890",
            LearningType = CocLearningType.Apprenticeship,
            ApprovalFieldChanges = [],
            Apprenticeship = new Apprenticeship
            {
                Id = 44021,
                PriceHistory =
                [
                    new PriceHistory { FromDate = new DateTime(2024, 8, 1), Cost = 1500 }
                ]
            }
        };
    }

    private void SetupRules(CocApprovalDetails details, CocApprovalResult result)
    {
        _cocApprovalRules.Setup(rules => rules.DetermineApprovalState(details)).ReturnsAsync(new CocApprovalState
        {
            ApprovalRequest = new ApprovalRequest
            {
                ApprenticeshipId = details.ApprenticeshipId,
                LearningType = details.LearningType,
                UKPRN = details.ProviderId.ToString(),
                ULN = details.ULN,
                Items = []
            },
            ApprovalResult = result
        });
    }
}
