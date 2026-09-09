using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeship;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands;

[TestFixture]
public class CocApprovalCommandHandlerTests
{
    private Mock<ICocApprovalRulesEngine> _cocApprovalRules;
    private Mock<ILogger<CocApprovalCommandHandler>> _logger;
    private ProviderCommitmentsDbContext _dbContext;
    private Lazy<ProviderCommitmentsDbContext> _lazyDbContext;
    private CocApprovalCommandHandler _sut;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ProviderCommitmentsDbContext(options);
        _lazyDbContext = new Lazy<ProviderCommitmentsDbContext>(() => _dbContext);

        _cocApprovalRules = new Mock<ICocApprovalRulesEngine>();
        _logger = new Mock<ILogger<CocApprovalCommandHandler>>();

        _sut = new CocApprovalCommandHandler(_lazyDbContext, _cocApprovalRules.Object, _logger.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }

    private static ApprovalRequest CreateApprovalRequest(Guid id, CocApprovalResultStatus status = CocApprovalResultStatus.Pending)
    {
        return new ApprovalRequest
        {
            Id = id,
            LearningKey = Guid.NewGuid(),
            Status = status,
            Items = new List<ApprovalFieldRequest>()
        };
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
    public async Task Handle_WhenActionIsCancelPrevious_AndPreviousRequestExists_RemovesItAndReturnsCancelledResult()
    {
        // Arrange
        var previousId = Guid.NewGuid();
        var existing = CreateApprovalRequest(previousId);
        await _dbContext.ApprovalRequests.AddAsync(existing);
        await _dbContext.SaveChangesAsync();

        var command = new CocApprovalCommand
        {
            Action = AggregrationAction.CancelPrevious,
            PreviousApprovalRequestId = previousId,
            CocApprovalDetails = null
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(CocApprovalResultStatus.Cancelled);
        result.Items.Should().NotBeNull().And.BeEmpty();

        existing.Status.Should().Be(CocApprovalResultStatus.Cancelled);

        _cocApprovalRules.Verify(r => r.DetermineApprovalState(It.IsAny<CocApprovalDetails>()), Times.Never);
    }

    [Test]
    public async Task Handle_WhenActionIsCancelPrevious_AndPreviousApprovalRequestIdIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var command = new CocApprovalCommand
        {
            Action = AggregrationAction.CancelPrevious,
            PreviousApprovalRequestId = null
        };

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("PreviousApprovalRequestId");

        _cocApprovalRules.Verify(r => r.DetermineApprovalState(It.IsAny<CocApprovalDetails>()), Times.Never);
    }

    [Test]
    public async Task Handle_WhenActionIsCancelPrevious_AndPreviousRequestDoesNotExist_Throws()
    {
        // Arrange: nothing seeded, FindAsync will return null, Remove(null) throws ArgumentNullException from EF
        var command = new CocApprovalCommand
        {
            Action = AggregrationAction.CancelPrevious,
            PreviousApprovalRequestId = Guid.NewGuid()
        };

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();

        _cocApprovalRules.Verify(r => r.DetermineApprovalState(It.IsAny<CocApprovalDetails>()), Times.Never);
    }

    [Test]
    public async Task Handle_WhenActionIsSupersedePrevious_AndPreviousRequestExists_MarksItSupersededAndReturnsRulesEngineResult()
    {
        // Arrange
        var previousId = Guid.NewGuid();
        var existing = CreateApprovalRequest(previousId, CocApprovalResultStatus.Pending);
        await _dbContext.ApprovalRequests.AddAsync(existing);
        await _dbContext.SaveChangesAsync();

        var details = new CocApprovalDetails { LearningKey = Guid.NewGuid() };
        var command = new CocApprovalCommand
        {
            Action = AggregrationAction.SupersedePrevious,
            PreviousApprovalRequestId = previousId,
            CocApprovalDetails = details
        };

        var newApprovalRequest = new ApprovalRequest { Id = Guid.NewGuid(), Items = new List<ApprovalFieldRequest>() };
        var expectedResult = new CocApprovalResult { Status = CocApprovalResultStatus.Pending, Items = new List<CocUpdateResult>() };
        var state = new CocApprovalState { ApprovalRequest = newApprovalRequest, ApprovalResult = expectedResult };

        _cocApprovalRules
            .Setup(r => r.DetermineApprovalState(details))
            .ReturnsAsync(state);

        var beforeCall = DateTime.UtcNow;

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(expectedResult);

        existing.Status.Should().Be(CocApprovalResultStatus.Superseded);
        existing.Updated.Should().NotBeNull();
        existing.Updated!.Value.Should().BeOnOrAfter(beforeCall);

        var supersededEntry = _dbContext.ChangeTracker.Entries<ApprovalRequest>()
            .First(e => e.Entity.Id == previousId);
        supersededEntry.State.Should().Be(EntityState.Modified);

        var addedEntry = _dbContext.ChangeTracker.Entries<ApprovalRequest>()
            .First(e => e.Entity.Id == newApprovalRequest.Id);
        addedEntry.State.Should().Be(EntityState.Added);

        _cocApprovalRules.Verify(r => r.DetermineApprovalState(details), Times.Once);
    }

    [Test]
    public async Task Handle_WhenActionIsSupersedePrevious_AndPreviousApprovalRequestIdIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var command = new CocApprovalCommand
        {
            Action = AggregrationAction.SupersedePrevious,
            PreviousApprovalRequestId = null
        };

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("PreviousApprovalRequestId");

        _cocApprovalRules.Verify(r => r.DetermineApprovalState(It.IsAny<CocApprovalDetails>()), Times.Never);
    }

    [Test]
    public async Task Handle_WhenActionIsSupersedePrevious_AndPreviousRequestDoesNotExist_ThrowsNullReferenceException()
    {
        // Arrange: nothing seeded, FindAsync returns null, MarkAsSuperseded dereferences .Status on null
        var command = new CocApprovalCommand
        {
            Action = AggregrationAction.SupersedePrevious,
            PreviousApprovalRequestId = Guid.NewGuid()
        };

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NullReferenceException>();

        _cocApprovalRules.Verify(r => r.DetermineApprovalState(It.IsAny<CocApprovalDetails>()), Times.Never);
    }

    [Test]
    public async Task Handle_WhenActionIsCreateNew_CallsRulesEngine_AddsReturnedApprovalRequest_AndReturnsResult()
    {
        // Arrange
        var details = new CocApprovalDetails { LearningKey = Guid.NewGuid() };
        var command = new CocApprovalCommand
        {
            Action = AggregrationAction.CreateNew,
            PreviousApprovalRequestId = null,
            CocApprovalDetails = details
        };

        var newApprovalRequest = new ApprovalRequest { Id = Guid.NewGuid(), Items = new List<ApprovalFieldRequest>() };
        var expectedResult = new CocApprovalResult { Status = CocApprovalResultStatus.Pending, Items = new List<CocUpdateResult>() };
        var state = new CocApprovalState { ApprovalRequest = newApprovalRequest, ApprovalResult = expectedResult };

        _cocApprovalRules
            .Setup(r => r.DetermineApprovalState(details))
            .ReturnsAsync(state);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(expectedResult);

        var addedEntry = _dbContext.ChangeTracker.Entries<ApprovalRequest>()
            .FirstOrDefault(e => e.Entity.Id == newApprovalRequest.Id);
        addedEntry.Should().NotBeNull();
        addedEntry!.State.Should().Be(EntityState.Added);

        _cocApprovalRules.Verify(r => r.DetermineApprovalState(details), Times.Once);
    }

    [Test]
    public async Task Handle_WhenActionIsCreateNew_DoesNotTouchAnyExistingApprovalRequest()
    {
        // Arrange: seed an unrelated pending request that should be left alone
        var unrelatedId = Guid.NewGuid();
        var unrelated = CreateApprovalRequest(unrelatedId, CocApprovalResultStatus.Pending);
        await _dbContext.ApprovalRequests.AddAsync(unrelated);
        await _dbContext.SaveChangesAsync();

        var details = new CocApprovalDetails { LearningKey = Guid.NewGuid() };
        var command = new CocApprovalCommand
        {
            Action = AggregrationAction.CreateNew,
            CocApprovalDetails = details
        };

        var newApprovalRequest = new ApprovalRequest { Id = Guid.NewGuid(), Items = new List<ApprovalFieldRequest>() };
        var state = new CocApprovalState
        {
            ApprovalRequest = newApprovalRequest,
            ApprovalResult = new CocApprovalResult { Status = CocApprovalResultStatus.Pending, Items = new List<CocUpdateResult>() }
        };

        _cocApprovalRules
            .Setup(r => r.DetermineApprovalState(details))
            .ReturnsAsync(state);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        unrelated.Status.Should().Be(CocApprovalResultStatus.Pending);
        var unrelatedEntry = _dbContext.ChangeTracker.Entries<ApprovalRequest>()
            .First(e => e.Entity.Id == unrelatedId);
        unrelatedEntry.State.Should().Be(EntityState.Unchanged);
    }
}
