using System;
using System.Linq;
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
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork.Context;

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
        _ = new UnitOfWorkContext();

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
                LogLevel.Information,
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
        var cohort = new Cohort
        {
            Id = _fixture.Create<long>(),
            WithParty = Party.Provider,
            Reference = _fixture.Create<string>()
        };
        
        var draftApprenticeship = new DraftApprenticeship
        {
            Id = _fixture.Create<long>(),
            LearnerDataId = message.LearnerId,
            HasLearnerDataChanges = false,
            FirstName = "Test",
            LastName = "User",
            DateOfBirth = DateTime.UtcNow.AddYears(-20),
            Uln = _fixture.Create<long>().ToString(),
            Cohort = cohort
        };

        _dbContext.Cohorts.Add(cohort);
        _dbContext.DraftApprenticeships.Add(draftApprenticeship);
        await _dbContext.SaveChangesAsync();

        // Act
        await _handler.ProcessLearnerDataChanges(message);

        // Assert
        var updatedApprenticeship = await _dbContext.DraftApprenticeships
            .FirstOrDefaultAsync(da => da.LearnerDataId == message.LearnerId);

        updatedApprenticeship.Should().NotBeNull();
        updatedApprenticeship.HasLearnerDataChanges.Should().BeTrue();

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
        var cohort1 = new Cohort
        {
            Id = _fixture.Create<long>(),
            WithParty = Party.Provider,
            Reference = _fixture.Create<string>()
        };
        var cohort2 = new Cohort
        {
            Id = _fixture.Create<long>(),
            WithParty = Party.Provider,
            Reference = _fixture.Create<string>()
        };
        
        var draftApprenticeship1 = new DraftApprenticeship
        {
            Id = _fixture.Create<long>(),
            LearnerDataId = message.LearnerId,
            HasLearnerDataChanges = false,
            FirstName = "Test1",
            LastName = "User1",
            DateOfBirth = DateTime.UtcNow.AddYears(-20),
            Uln = _fixture.Create<long>().ToString(),
            Cohort = cohort1
        };
        var draftApprenticeship2 = new DraftApprenticeship
        {
            Id = _fixture.Create<long>(),
            LearnerDataId = message.LearnerId,
            HasLearnerDataChanges = false,
            FirstName = "Test2",
            LastName = "User2",
            DateOfBirth = DateTime.UtcNow.AddYears(-21),
            Uln = _fixture.Create<long>().ToString(),
            Cohort = cohort2
        };

        _dbContext.Cohorts.AddRange(cohort1, cohort2);
        _dbContext.DraftApprenticeships.AddRange(draftApprenticeship1, draftApprenticeship2);
        await _dbContext.SaveChangesAsync();

        // Act
        await _handler.ProcessLearnerDataChanges(message);

        // Assert
        var updatedApprenticeships = await _dbContext.DraftApprenticeships
            .Where(da => da.LearnerDataId == message.LearnerId)
            .ToListAsync();

        updatedApprenticeships.Should().HaveCount(2);
        
        var updatedApprenticeship = updatedApprenticeships.FirstOrDefault(da => da.HasLearnerDataChanges);
        var unchangedApprenticeship = updatedApprenticeships.FirstOrDefault(da => !da.HasLearnerDataChanges);
        
        updatedApprenticeship.Should().NotBeNull();
        updatedApprenticeship.HasLearnerDataChanges.Should().BeTrue();
        
        unchangedApprenticeship.Should().NotBeNull();
        unchangedApprenticeship.HasLearnerDataChanges.Should().BeFalse();
        unchangedApprenticeship.LastLearnerDataSync.Should().BeNull();
    }

    [Test]
    public async Task ProcessLearnerDataChanges_WhenAlreadyFlagged_UpdatesChangeDate()
    {
        // Arrange
        var message = _fixture.Create<LearnerDataUpdatedEvent>();
        var cohort = new Cohort
        {
            Id = _fixture.Create<long>(),
            WithParty = Party.Provider,
            Reference = _fixture.Create<string>()
        };
        
        var draftApprenticeship = new DraftApprenticeship
        {
            Id = _fixture.Create<long>(),
            LearnerDataId = message.LearnerId,
            HasLearnerDataChanges = true,
            LastLearnerDataSync = null,
            FirstName = "Test",
            LastName = "User",
            DateOfBirth = DateTime.UtcNow.AddYears(-20),
            Uln = _fixture.Create<long>().ToString(),
            Cohort = cohort
        };

        _dbContext.Cohorts.Add(cohort);
        _dbContext.DraftApprenticeships.Add(draftApprenticeship);
        await _dbContext.SaveChangesAsync();

        // Act
        await _handler.ProcessLearnerDataChanges(message);

        // Assert
        var updatedApprenticeship = await _dbContext.DraftApprenticeships
            .FirstOrDefaultAsync(da => da.LearnerDataId == message.LearnerId);

        updatedApprenticeship.Should().NotBeNull();
        updatedApprenticeship.HasLearnerDataChanges.Should().BeTrue();
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
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"No draft apprenticeship found for learner {message.LearnerId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Test]
    public async Task ProcessLearnerDataChanges_WhenCohortWithEmployer_TransitionsBackToProvider()
    {
        // Arrange
        var message = _fixture.Create<LearnerDataUpdatedEvent>();
        var cohort = new Cohort
        {
            Id = _fixture.Create<long>(),
            WithParty = Party.Employer,
            Reference = _fixture.Create<string>()
        };
        
        var draftApprenticeship = new DraftApprenticeship
        {
            Id = _fixture.Create<long>(),
            LearnerDataId = message.LearnerId,
            HasLearnerDataChanges = false,
            FirstName = "Test",
            LastName = "User",
            DateOfBirth = DateTime.UtcNow.AddYears(-20),
            Uln = _fixture.Create<long>().ToString(),
            Cohort = cohort
        };

        _dbContext.Cohorts.Add(cohort);
        _dbContext.DraftApprenticeships.Add(draftApprenticeship);
        await _dbContext.SaveChangesAsync();

        // Act
        await _handler.ProcessLearnerDataChanges(message);

        // Assert
        var updatedCohort = await _dbContext.Cohorts
            .FirstOrDefaultAsync(c => c.Id == cohort.Id);

        updatedCohort.Should().NotBeNull();
        updatedCohort.WithParty.Should().Be(Party.Provider);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Cohort {cohort.Id} is WithEmployer, transitioning back to WithProvider due to learner data changes")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Successfully transitioned cohort {cohort.Id} from WithEmployer to WithProvider")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Test]
    public async Task ProcessLearnerDataChanges_WhenCohortWithTransferSender_TransitionsBackToProvider()
    {
        // Arrange
        var message = _fixture.Create<LearnerDataUpdatedEvent>();
        var cohort = new Cohort
        {
            Id = _fixture.Create<long>(),
            WithParty = Party.TransferSender,
            Reference = _fixture.Create<string>()
        };
        
        var draftApprenticeship = new DraftApprenticeship
        {
            Id = _fixture.Create<long>(),
            LearnerDataId = message.LearnerId,
            HasLearnerDataChanges = false,
            FirstName = "Test",
            LastName = "User",
            DateOfBirth = DateTime.UtcNow.AddYears(-20),
            Uln = _fixture.Create<long>().ToString(),
            Cohort = cohort
        };

        _dbContext.Cohorts.Add(cohort);
        _dbContext.DraftApprenticeships.Add(draftApprenticeship);
        await _dbContext.SaveChangesAsync();

        // Act
        await _handler.ProcessLearnerDataChanges(message);

        // Assert
        var updatedCohort = await _dbContext.Cohorts
            .FirstOrDefaultAsync(c => c.Id == cohort.Id);

        updatedCohort.Should().NotBeNull();
        updatedCohort.WithParty.Should().Be(Party.Provider);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Cohort {cohort.Id} is WithTransferSender, transitioning back to WithProvider due to learner data changes")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Successfully transitioned cohort {cohort.Id} from WithTransferSender to WithProvider")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
} 