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
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.ExternalHandlers;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.ExternalMessageHandlers.UnitTests.EventHandlers;

public class LearningPausedEventHandlerTests
{
    public LearningPausedEventHandlerTestsFixture _fixture;

    [SetUp]
    public void Arrange()
    {
        _fixture = new LearningPausedEventHandlerTestsFixture();
    }

    [Test]
    public async Task Handle_WhenLearningPausedEventReceived()
    {
        await _fixture.Handle();
        _fixture.VerifyLearnerPaused();
        _fixture.VerifyStoreLearnerHistoryCommandIsSent();
        _fixture.VerifyPauseDateIsAssignedCorrectly();
    }

    [Test]
    public async Task ThenThrowsDomainException_WhenPauseDateIsEarlierThanLearningStartDate()
    {
        var act = async () => await _fixture.SetStartDate(DateTime.UtcNow.AddMonths(4)).Handle();
        await act.Should().ThrowAsync<DomainException>().Where(ex => ex.DomainErrors.First().ErrorMessage.Contains("Learner not started"));
    }

    [Test]
    public async Task ThenThrowsDomainException_WhenPauseDateIsAfterLearningEndDate()
    {
        var act = async () => await _fixture.SetEndDate(DateTime.UtcNow.AddMonths(-2)).Handle();
        await act.Should().ThrowAsync<DomainException>().Where(ex => ex.DomainErrors.First().ErrorMessage.Contains("Pause date cannot be on or after the end date"));
        _fixture.VerifyApprenticeshipPauseDateChangedEventIsNotPublished();
    }

    [Test]
    public async Task ThenThrowsDomainException_WhenPauseDateIsOnLearningEndDate()
    {
        var act = async () => await _fixture.SetEndDate(DateTime.UtcNow.AddMonths(3)).Handle();
        await act.Should().ThrowAsync<DomainException>().Where(ex => ex.DomainErrors.First().ErrorMessage.Contains("Pause date cannot be on or after the end date"));
    }

    [Test]
    public async Task ThenThrowsDomainException_WhenStatusIsCompleted()
    {
        var act = async () => await _fixture.SetPaymentStatus(PaymentStatus.Completed).Handle();
        await act.Should().ThrowAsync<DomainException>().Where(ex => ex.DomainErrors.First().ErrorMessage.Contains("Learning cannot be Paused if Payment Status is Completed or Withdrawn"));
    }
}

public class LearningPausedEventHandlerTestsFixture
{
    private Fixture _fixture;
    private ProviderCommitmentsDbContext _dbContext;
    private Mock<ILogger<LearningPausedEventHandler>> _mockLogger;
    private Mock<IMessageSession> _mockSession;
    private LearningPausedEventHandler _handler;
    private Mock<IMessageHandlerContext> _mockContext;
    private LearningPausedEvent _event;
    private Mock<IChangeTrackingSession> _mocktrackingSession;
    public UnitOfWorkContext UnitOfWorkContext { get; set; }
    public long apprenticeshipId { get; set; }

    public LearningPausedEventHandlerTestsFixture()
    {
        _fixture = new Fixture();
        _mockLogger = new Mock<ILogger<LearningPausedEventHandler>>();
        _mockSession = new Mock<IMessageSession>();
        _mockContext = new Mock<IMessageHandlerContext>();
        _mocktrackingSession = new Mock<IChangeTrackingSession>();
        UnitOfWorkContext = new UnitOfWorkContext();

        _event = _fixture.Create<LearningPausedEvent>();

        var options = new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                                            .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
                                            .Options);

        apprenticeshipId = _fixture.Create<long>();
        _event.ApprenticeshipId = apprenticeshipId;
        _event.PauseDate = DateTime.UtcNow.AddMonths(3);

        var provider = new Provider()
        {
            UkPrn = 12345,
            Name = "Test Provider"
        };

        var cohort = new Cohort
        {
            Id = _fixture.Create<long>(),
            WithParty = Party.Provider,
            Reference = _fixture.Create<string>(),
            Provider = provider,
            EmployerAccountId = 101
        };

        var Apprenticeship = new Apprenticeship
        {
            Id = apprenticeshipId,
            HasLearnerDataChanges = false,
            FirstName = "Test",
            LastName = "User",
            DateOfBirth = DateTime.UtcNow.AddYears(-20),
            Uln = _fixture.Create<long>().ToString(),
            Cohort = cohort,
            StartDate = DateTime.UtcNow.AddMonths(1),
            EndDate = DateTime.UtcNow.AddMonths(13)
        };

        _dbContext.Cohorts.Add(cohort);
        _dbContext.Apprenticeships.Add(Apprenticeship);
        _ = _dbContext.SaveChangesAsync();

        _handler = new LearningPausedEventHandler(new Lazy<ProviderCommitmentsDbContext>(() => _dbContext), _mockSession.Object, _mockLogger.Object);
    }

    public LearningPausedEventHandlerTestsFixture SetStartDate(DateTime startDate)
    {
        var apprenticeship = _dbContext.Apprenticeships.Find(apprenticeshipId);
        apprenticeship.StartDate = startDate;
        _dbContext.SaveChangesAsync();
        return this;
    }

    public LearningPausedEventHandlerTestsFixture SetEndDate(DateTime endDate)
    {
        var apprenticeship = _dbContext.Apprenticeships.Find(apprenticeshipId);
        apprenticeship.EndDate = endDate;
        _dbContext.SaveChangesAsync();
        return this;
    }

    public LearningPausedEventHandlerTestsFixture SetPaymentStatus(PaymentStatus status)
    {
        var apprenticeship = _dbContext.Apprenticeships.Find(apprenticeshipId);
        apprenticeship.PaymentStatus = status;
        _dbContext.SaveChangesAsync();
        return this;
    }

    public async Task Handle()
    {
        await _handler.Handle(_event, _mockContext.Object);
    }

    public void VerifyLogger()
    {
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Handling LearningPausedEvent for apprenticeship {_event.ApprenticeshipId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    public void VerifyStoreLearnerHistoryCommandIsSent()
    {
        _mockSession.Verify(x => x.Send(It.Is<StoreLearningHistoryCommand>(c =>
            c.ApprenticeshipId == _event.ApprenticeshipId &&
            c.Source == Types.LearningSourceType.ILRStatusChange &&
            c.ChangeType == Types.LearningChangeType.AutoApproved &&
            c.LearningKey == _event.LearningKey &&
            c.AppliedDate == _event.Created &&
            c.Description == $"Learning has been paused on {_event.PauseDate}"
        ), It.IsAny<SendOptions>()), Times.Once);
    }

    public void VerifyPauseDateIsAssignedCorrectly()
    {
        var apprenticeship = _dbContext.Apprenticeships.Find(apprenticeshipId);
        apprenticeship.PauseDate.Should().Be(_event.PauseDate.Date);
    }

    public void VerifyApprenticeshipPauseDateChangedEventIsCorrectlyPublished()
    {
        var apprenticeship = _dbContext.Apprenticeships.Find(_event.ApprenticeshipId);
        var pausedEvent = UnitOfWorkContext.GetEvents().OfType<ApprenticeshipPausedEvent>().First();
        pausedEvent.Should().NotBeNull();
        pausedEvent.PausedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        pausedEvent.ApprenticeshipId.Should().Be(_event.ApprenticeshipId);
        pausedEvent.PausedViaILR.Should().BeTrue();
    }

    public void VerifyApprenticeshipPauseDateChangedEventIsNotPublished()
    {
        var pausedEvent = UnitOfWorkContext.GetEvents().OfType<ApprenticeshipStopDateChangedEvent>().FirstOrDefault();
        pausedEvent.Should().BeNull();
    }

    public void VerifyLearnerPaused()
    {
        var updatedApprenticeship = _dbContext.Apprenticeships.Find(apprenticeshipId);
        updatedApprenticeship.Should().NotBeNull();
        updatedApprenticeship.PaymentStatus.Should().Be(PaymentStatus.Paused);
        updatedApprenticeship.PauseDate.Should().HaveValue();
    }

    public void VerifyHasError()
    {
        _mockLogger.Verify(
           x => x.Log(
               LogLevel.Error,
               It.IsAny<EventId>(),
               It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Handling LearningPausedEvent for apprenticeship {_event.ApprenticeshipId}")),
               It.IsAny<Exception>(),
               It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Once);
    }
}