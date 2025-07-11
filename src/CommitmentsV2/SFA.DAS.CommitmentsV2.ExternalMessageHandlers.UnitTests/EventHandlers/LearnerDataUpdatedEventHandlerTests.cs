using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.ExternalHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.ExternalHandlers.Messages;

namespace SFA.DAS.CommitmentsV2.ExternalMessageHandlers.UnitTests.EventHandlers;

[TestFixture]
public class LearnerDataUpdatedEventHandlerTests
{
    [Test]
    public async Task Handle_WhenEventHasChanges_ShouldLogChangesAndProcess()
    {
        // Arrange
        var @event = new LearnerDataUpdatedEvent
        {
            LearnerId = 123,
            ChangeSummary = new ChangeSummary
            {
                Changes = new List<FieldChange>
                {
                    new() { FieldName = "FirstName", OldValue = "John", NewValue = "Jonathan" },
                    new() { FieldName = "Email", OldValue = "john@example.com", NewValue = "jonathan@example.com" }
                }
            }
        };
        var mockLogger = new Mock<ILogger<LearnerDataUpdatedEventHandler>>();
        var mockContext = new Mock<IMessageHandlerContext>();
        var handler = new LearnerDataUpdatedEventHandler(mockLogger.Object);

        // Act
        await handler.Handle(@event, mockContext.Object);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Handling LearnerDataUpdatedEvent")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("FirstName")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Email")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_WhenEventHasNoChanges_ShouldLogNoChangesMessage()
    {
        // Arrange
        var @event = new LearnerDataUpdatedEvent
        {
            LearnerId = 456,
            ChangeSummary = new ChangeSummary { Changes = new List<FieldChange>() }
        };
        var mockLogger = new Mock<ILogger<LearnerDataUpdatedEventHandler>>();
        var mockContext = new Mock<IMessageHandlerContext>();
        var handler = new LearnerDataUpdatedEventHandler(mockLogger.Object);

        // Act
        await handler.Handle(@event, mockContext.Object);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("has no changes to process")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public void Handle_WhenExceptionOccurs_ShouldLogErrorAndRethrow()
    {
        // Arrange
        var @event = new LearnerDataUpdatedEvent
        {
            LearnerId = 789,
            ChangeSummary = new ChangeSummary
            {
                Changes = new List<FieldChange>
                {
                    new() { FieldName = "FirstName", OldValue = "John", NewValue = "Jonathan" }
                }
            }
        };
        var mockLogger = new Mock<ILogger<LearnerDataUpdatedEventHandler>>();
        var mockContext = new Mock<IMessageHandlerContext>();
        var handler = new LearnerDataUpdatedEventHandler(mockLogger.Object);

        // Simulate exception by replacing ProcessLearnerDataChanges with a throwing version
        var ex = new Exception("Test exception");
        var handlerWithThrow = new TestLearnerDataUpdatedEventHandler(mockLogger.Object, ex);

        // Act & Assert
        var thrown = Assert.ThrowsAsync<Exception>(() => handlerWithThrow.Handle(@event, mockContext.Object));
        Assert.That(thrown, Is.EqualTo(ex));
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error processing LearnerDataUpdatedEvent")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private class TestLearnerDataUpdatedEventHandler : LearnerDataUpdatedEventHandler
    {
        private readonly Exception _exception;
        public TestLearnerDataUpdatedEventHandler(ILogger<LearnerDataUpdatedEventHandler> logger, Exception exception)
            : base(logger) => _exception = exception;
        protected override Task ProcessLearnerDataChanges(LearnerDataUpdatedEvent message) => Task.FromException(_exception);
    }
} 