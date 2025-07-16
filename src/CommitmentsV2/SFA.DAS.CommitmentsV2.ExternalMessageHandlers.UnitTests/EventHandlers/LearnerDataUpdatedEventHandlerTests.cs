using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.ExternalHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.ExternalHandlers.Messages;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.ExternalMessageHandlers.UnitTests.EventHandlers;

[TestFixture]
public class LearnerDataUpdatedEventHandlerTests
{
    private Lazy<ProviderCommitmentsDbContext> _dbContext;
    private Mock<ProviderCommitmentsDbContext> _mockContext;
    private Mock<ILogger<LearnerDataUpdatedEventHandler>> _mockLogger;
    private LearnerDataUpdatedEventHandler _handler;
    private Mock<IMessageHandlerContext> _mockMessageContext;

    [SetUp]
    public void Setup()
    {
        _mockContext = new Mock<ProviderCommitmentsDbContext>();
        _mockLogger = new Mock<ILogger<LearnerDataUpdatedEventHandler>>();
        _mockMessageContext = new Mock<IMessageHandlerContext>();

        // Create a real Lazy instance that returns our mock context
        _dbContext = new Lazy<ProviderCommitmentsDbContext>(() => _mockContext.Object);

        _handler = new LearnerDataUpdatedEventHandler(_dbContext, _mockLogger.Object);
    }

    [Test]
    public async Task Handle_WhenLearnerDataUpdatedEventReceived_LogsInformation()
    {
        // Arrange
        var message = new LearnerDataUpdatedEvent
        {
            LearnerId = 123,
            ChangeSummary = new ChangeSummary
            {
                Changes = new List<FieldChange>
                {
                    new() { FieldName = "FirstName", OldValue = "John", NewValue = "Jonathan" }
                }
            }
        };

        var mockDbSet = new Mock<DbSet<DraftApprenticeship>>();
        _mockContext.Setup(x => x.DraftApprenticeships).Returns(mockDbSet.Object);

        // Act
        await _handler.Handle(message, _mockMessageContext.Object);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Handling LearnerDataUpdatedEvent")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_WhenNoDraftApprenticeshipsFound_LogsWarning()
    {
        // Arrange
        var message = new LearnerDataUpdatedEvent
        {
            LearnerId = 123,
            ChangeSummary = new ChangeSummary
            {
                Changes = new List<FieldChange>
                {
                    new() { FieldName = "FirstName", OldValue = "John", NewValue = "Jonathan" }
                }
            }
        };

        var mockDbSet = new Mock<DbSet<DraftApprenticeship>>();
        var emptyList = new List<DraftApprenticeship>().AsQueryable();
        mockDbSet.As<IQueryable<DraftApprenticeship>>().Setup(m => m.Provider).Returns(emptyList.Provider);
        mockDbSet.As<IQueryable<DraftApprenticeship>>().Setup(m => m.Expression).Returns(emptyList.Expression);
        mockDbSet.As<IQueryable<DraftApprenticeship>>().Setup(m => m.ElementType).Returns(emptyList.ElementType);
        mockDbSet.As<IQueryable<DraftApprenticeship>>().Setup(m => m.GetEnumerator()).Returns(emptyList.GetEnumerator());

        _mockContext.Setup(x => x.DraftApprenticeships).Returns(mockDbSet.Object);

        // Act
        await _handler.Handle(message, _mockMessageContext.Object);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No draft apprenticeships found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_WhenExceptionOccurs_LogsError()
    {
        // Arrange
        var message = new LearnerDataUpdatedEvent
        {
            LearnerId = 123,
            ChangeSummary = new ChangeSummary
            {
                Changes = new List<FieldChange>
                {
                    new() { FieldName = "FirstName", OldValue = "John", NewValue = "Jonathan" }
                }
            }
        };

        _mockContext.Setup(x => x.DraftApprenticeships).Throws(new Exception("Database error"));

        // Act
        await _handler.Handle(message, _mockMessageContext.Object);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error handling LearnerDataUpdatedEvent")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Test]
    public async Task ProcessLearnerDataChanges_WhenDraftApprenticeshipsFound_FlagsThemCorrectly()
    {
        // Arrange
        var message = new LearnerDataUpdatedEvent
        {
            LearnerId = 123,
            ChangeSummary = new ChangeSummary
            {
                Changes = new List<FieldChange>
                {
                    new() { FieldName = "FirstName", OldValue = "John", NewValue = "Jonathan" }
                }
            }
        };

        var draftApprenticeships = new List<DraftApprenticeship>
        {
            new() { Id = 1, LearnerDataId = 123, HasLearnerDataChanges = false },
            new() { Id = 2, LearnerDataId = 123, HasLearnerDataChanges = false },
            new() { Id = 3, LearnerDataId = 456, HasLearnerDataChanges = false } // Different learner data ID
        };

        var mockDbSet = new Mock<DbSet<DraftApprenticeship>>();
        var queryableList = draftApprenticeships.AsQueryable();
        mockDbSet.As<IQueryable<DraftApprenticeship>>().Setup(m => m.Provider).Returns(queryableList.Provider);
        mockDbSet.As<IQueryable<DraftApprenticeship>>().Setup(m => m.Expression).Returns(queryableList.Expression);
        mockDbSet.As<IQueryable<DraftApprenticeship>>().Setup(m => m.ElementType).Returns(queryableList.ElementType);
        mockDbSet.As<IQueryable<DraftApprenticeship>>().Setup(m => m.GetEnumerator()).Returns(queryableList.GetEnumerator());

        _mockContext.Setup(x => x.DraftApprenticeships).Returns(mockDbSet.Object);

        // Act
        await _handler.ProcessLearnerDataChanges(message);

        // Assert
        Assert.That(draftApprenticeships[0].HasLearnerDataChanges, Is.True);
        Assert.That(draftApprenticeships[0].LearnerDataChangeDate, Is.Not.Null);
        Assert.That(draftApprenticeships[1].HasLearnerDataChanges, Is.True);
        Assert.That(draftApprenticeships[1].LearnerDataChangeDate, Is.Not.Null);
        Assert.That(draftApprenticeships[2].HasLearnerDataChanges, Is.False);
        Assert.That(draftApprenticeships[2].LearnerDataChangeDate, Is.Null);
    }

    [Test]
    public async Task ProcessLearnerDataChanges_WhenNoChanges_DoesNotFlagApprenticeships()
    {
        // Arrange
        var message = new LearnerDataUpdatedEvent
        {
            LearnerId = 123,
            ChangeSummary = new ChangeSummary
            {
                Changes = new List<FieldChange>() // Empty list means no changes
            }
        };

        var draftApprenticeships = new List<DraftApprenticeship>
        {
            new() { Id = 1, LearnerDataId = 123, HasLearnerDataChanges = false }
        };

        var mockDbSet = new Mock<DbSet<DraftApprenticeship>>();
        var queryableList = draftApprenticeships.AsQueryable();
        mockDbSet.As<IQueryable<DraftApprenticeship>>().Setup(m => m.Provider).Returns(queryableList.Provider);
        mockDbSet.As<IQueryable<DraftApprenticeship>>().Setup(m => m.Expression).Returns(queryableList.Expression);
        mockDbSet.As<IQueryable<DraftApprenticeship>>().Setup(m => m.ElementType).Returns(queryableList.ElementType);
        mockDbSet.As<IQueryable<DraftApprenticeship>>().Setup(m => m.GetEnumerator()).Returns(queryableList.GetEnumerator());

        _mockContext.Setup(x => x.DraftApprenticeships).Returns(mockDbSet.Object);

        // Act
        await _handler.ProcessLearnerDataChanges(message);

        // Assert
        Assert.That(draftApprenticeships[0].HasLearnerDataChanges, Is.False);
        Assert.That(draftApprenticeships[0].LearnerDataChangeDate, Is.Null);
    }

    [Test]
    public async Task ProcessLearnerDataChanges_WhenAlreadyFlagged_UpdatesChangeDate()
    {
        // Arrange
        var originalChangeDate = DateTime.UtcNow.AddDays(-1);
        var message = new LearnerDataUpdatedEvent
        {
            LearnerId = 123,
            ChangeSummary = new ChangeSummary
            {
                Changes = new List<FieldChange>
                {
                    new() { FieldName = "FirstName", OldValue = "John", NewValue = "Jonathan" }
                }
            }
        };

        var draftApprenticeships = new List<DraftApprenticeship>
        {
            new() 
            { 
                Id = 1, 
                LearnerDataId = 123, 
                HasLearnerDataChanges = true,
                LearnerDataChangeDate = originalChangeDate
            }
        };

        var mockDbSet = new Mock<DbSet<DraftApprenticeship>>();
        var queryableList = draftApprenticeships.AsQueryable();
        mockDbSet.As<IQueryable<DraftApprenticeship>>().Setup(m => m.Provider).Returns(queryableList.Provider);
        mockDbSet.As<IQueryable<DraftApprenticeship>>().Setup(m => m.Expression).Returns(queryableList.Expression);
        mockDbSet.As<IQueryable<DraftApprenticeship>>().Setup(m => m.ElementType).Returns(queryableList.ElementType);
        mockDbSet.As<IQueryable<DraftApprenticeship>>().Setup(m => m.GetEnumerator()).Returns(queryableList.GetEnumerator());

        _mockContext.Setup(x => x.DraftApprenticeships).Returns(mockDbSet.Object);

        // Act
        await _handler.ProcessLearnerDataChanges(message);

        // Assert
        Assert.That(draftApprenticeships[0].HasLearnerDataChanges, Is.True);
        Assert.That(draftApprenticeships[0].LearnerDataChangeDate, Is.Not.Null);
        Assert.That(draftApprenticeships[0].LearnerDataChangeDate, Is.GreaterThan(originalChangeDate));
    }

    [Test]
    public async Task ProcessLearnerDataChanges_WhenNoDraftApprenticeshipsFound_LogsWarning()
    {
        // Arrange
        var message = new LearnerDataUpdatedEvent
        {
            LearnerId = 123,
            ChangeSummary = new ChangeSummary
            {
                Changes = new List<FieldChange>
                {
                    new() { FieldName = "FirstName", OldValue = "John", NewValue = "Jonathan" }
                }
            }
        };

        var mockDbSet = new Mock<DbSet<DraftApprenticeship>>();
        var emptyList = new List<DraftApprenticeship>().AsQueryable();
        mockDbSet.As<IQueryable<DraftApprenticeship>>().Setup(m => m.Provider).Returns(emptyList.Provider);
        mockDbSet.As<IQueryable<DraftApprenticeship>>().Setup(m => m.Expression).Returns(emptyList.Expression);
        mockDbSet.As<IQueryable<DraftApprenticeship>>().Setup(m => m.ElementType).Returns(emptyList.ElementType);
        mockDbSet.As<IQueryable<DraftApprenticeship>>().Setup(m => m.GetEnumerator()).Returns(emptyList.GetEnumerator());

        _mockContext.Setup(x => x.DraftApprenticeships).Returns(mockDbSet.Object);

        // Act
        await _handler.ProcessLearnerDataChanges(message);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No draft apprenticeships found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
} 