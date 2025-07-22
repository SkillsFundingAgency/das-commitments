using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.ExternalHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.ExternalHandlers.Messages;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.ExternalMessageHandlers.UnitTests.EventHandlers;

[TestFixture]
public class LearnerDataUpdatedEventHandlerTests
{
    private Fixture _fixture;
    private ProviderCommitmentsDbContext _dbContext;
    private Mock<ILogger<LearnerDataUpdatedEventHandler>> _mockLogger;
    private Mock<IMessageHandlerContext> _mockContext;
    private LearnerDataUpdatedEventHandler _handler;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _mockLogger = new Mock<ILogger<LearnerDataUpdatedEventHandler>>();
        _mockContext = new Mock<IMessageHandlerContext>();

        // Use in-memory database for testing
        var options = new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ProviderCommitmentsDbContext(options);
        _handler = new LearnerDataUpdatedEventHandler(new Lazy<ProviderCommitmentsDbContext>(() => _dbContext), _mockLogger.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext?.Dispose();
    }

    [Test]
    public async Task Handle_WhenLearnerDataUpdatedEventReceived_LogsInformation()
    {
        // Arrange
        var message = _fixture.Create<LearnerDataUpdatedEvent>();

        // Act
        await _handler.Handle(message, _mockContext.Object);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Handling LearnerDataUpdatedEvent for learner {message.LearnerId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_WhenNoDraftApprenticeshipsFound_LogsWarning()
    {
        // Arrange
        var message = _fixture.Create<LearnerDataUpdatedEvent>();

        // Act
        await _handler.Handle(message, _mockContext.Object);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"No draft apprenticeship found for learner {message.LearnerId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Test]
    public async Task ProcessLearnerDataChanges_WhenDraftApprenticeshipFound_FlagsItCorrectly()
    {
        // Arrange
        var message = _fixture.Create<LearnerDataUpdatedEvent>();
        var draftApprenticeship = new DraftApprenticeship
        {
            Id = _fixture.Create<long>(),
            LearnerDataId = message.LearnerId,
            HasLearnerDataChanges = false,
            FirstName = "Test",
            LastName = "User",
            DateOfBirth = DateTime.UtcNow.AddYears(-20),
            Uln = _fixture.Create<long>().ToString()
        };

        _dbContext.DraftApprenticeships.Add(draftApprenticeship);
        await _dbContext.SaveChangesAsync();

        // Act
        await _handler.ProcessLearnerDataChanges(message);

        // Assert
        var updatedApprenticeship = await _dbContext.DraftApprenticeships
            .FirstOrDefaultAsync(da => da.LearnerDataId == message.LearnerId);

        updatedApprenticeship.Should().NotBeNull();
        updatedApprenticeship.HasLearnerDataChanges.Should().BeTrue();
        updatedApprenticeship.LastLearnerDataSync.Should().Be(message.ChangedAt);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Flagged draft apprenticeship {draftApprenticeship.Id} for learner data changes")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Test]
    public async Task ProcessLearnerDataChanges_WhenMultipleDraftApprenticeshipsFound_FlagsFirstOneCorrectly()
    {
        // Arrange
        var message = _fixture.Create<LearnerDataUpdatedEvent>();
        var draftApprenticeship1 = new DraftApprenticeship
        {
            Id = _fixture.Create<long>(),
            LearnerDataId = message.LearnerId,
            HasLearnerDataChanges = false,
            FirstName = "Test1",
            LastName = "User1",
            DateOfBirth = DateTime.UtcNow.AddYears(-20),
            Uln = _fixture.Create<long>().ToString()
        };
        var draftApprenticeship2 = new DraftApprenticeship
        {
            Id = _fixture.Create<long>(),
            LearnerDataId = message.LearnerId,
            HasLearnerDataChanges = false,
            FirstName = "Test2",
            LastName = "User2",
            DateOfBirth = DateTime.UtcNow.AddYears(-21),
            Uln = _fixture.Create<long>().ToString()
        };

        _dbContext.DraftApprenticeships.AddRange(draftApprenticeship1, draftApprenticeship2);
        await _dbContext.SaveChangesAsync();

        // Act
        await _handler.ProcessLearnerDataChanges(message);

        // Assert
        var updatedApprenticeships = await _dbContext.DraftApprenticeships
            .Where(da => da.LearnerDataId == message.LearnerId)
            .ToListAsync();

        updatedApprenticeships.Should().HaveCount(2);
        
        // One should be updated, one should remain unchanged
        var updatedApprenticeship = updatedApprenticeships.FirstOrDefault(da => da.HasLearnerDataChanges);
        var unchangedApprenticeship = updatedApprenticeships.FirstOrDefault(da => !da.HasLearnerDataChanges);
        
        updatedApprenticeship.Should().NotBeNull();
        updatedApprenticeship.HasLearnerDataChanges.Should().BeTrue();
        updatedApprenticeship.LastLearnerDataSync.Should().Be(message.ChangedAt);
        
        unchangedApprenticeship.Should().NotBeNull();
        unchangedApprenticeship.HasLearnerDataChanges.Should().BeFalse();
        unchangedApprenticeship.LastLearnerDataSync.Should().BeNull();
    }

    [Test]
    public async Task ProcessLearnerDataChanges_WhenAlreadyFlagged_UpdatesChangeDate()
    {
        // Arrange
        var message = _fixture.Create<LearnerDataUpdatedEvent>();
        var originalChangeDate = DateTime.UtcNow.AddDays(-1);
        var draftApprenticeship = new DraftApprenticeship
        {
            Id = _fixture.Create<long>(),
            LearnerDataId = message.LearnerId,
            HasLearnerDataChanges = true,
            LastLearnerDataSync = originalChangeDate,
            FirstName = "Test",
            LastName = "User",
            DateOfBirth = DateTime.UtcNow.AddYears(-20),
            Uln = _fixture.Create<long>().ToString()
        };

        _dbContext.DraftApprenticeships.Add(draftApprenticeship);
        await _dbContext.SaveChangesAsync();

        // Act
        await _handler.ProcessLearnerDataChanges(message);

        // Assert
        var updatedApprenticeship = await _dbContext.DraftApprenticeships
            .FirstOrDefaultAsync(da => da.LearnerDataId == message.LearnerId);

        updatedApprenticeship.Should().NotBeNull();
        updatedApprenticeship.HasLearnerDataChanges.Should().BeTrue();
        updatedApprenticeship.LastLearnerDataSync.Should().Be(message.ChangedAt);
        updatedApprenticeship.LastLearnerDataSync.Should().NotBe(originalChangeDate);
    }

    [Test]
    public async Task ProcessLearnerDataChanges_WhenNoDraftApprenticeshipsFound_LogsWarning()
    {
        // Arrange
        var message = _fixture.Create<LearnerDataUpdatedEvent>();

        // Act
        await _handler.ProcessLearnerDataChanges(message);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"No draft apprenticeship found for learner {message.LearnerId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
} 