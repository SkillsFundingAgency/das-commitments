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
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.ExternalHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Extensions;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;
using SFA.DAS.Learning.Types;
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

    [TearDown]
    public void TearDown() => _fixture.Dispose();

    [Test]
    public async Task Handle_WhenLearningPausedEventReceived()
    {
        await _fixture.Handle();
        _fixture.VerifyLearnerPaused();
        _fixture.VerifyStoreLearnerHistoryCommandIsSent();
        _fixture.VerifyLearningPausedEventIsPublished();
        _fixture.VerifySendEmailToEmployerCommandIsSent();
    }

    [Test]
    public async Task ThenThrowsDomainException_WhenPauseDateIsEarlierThanLearningStartDate()
    {
        var act = async () => await _fixture.SetStartDate(DateTime.UtcNow.AddMonths(4)).Handle();
        await act.Should().ThrowAsync<DomainException>().Where(ex => ex.DomainErrors.First().ErrorMessage.Contains("Learner not started"));
        _fixture.VerifyLearningPausedEventIsNotPublished();
    }

    [Test]
    public async Task ThenThrowsDomainException_WhenApprenticeshipNotfound()
    {
        var act = async () => await _fixture.SetEventApprenticeshipId(_fixture.fixture.Create<long>()).Handle();
        await act.Should().ThrowAsync<DomainException>().Where(ex => ex.DomainErrors.First().ErrorMessage.Contains("not found"));
        _fixture.VerifyLearningPausedEventIsNotPublished();
    }

    [Test]
    public async Task ThenThrowsDomainException_WhenPauseDateIsAfterLearningEndDate()
    {
        var act = async () => await _fixture.SetEndDate(DateTime.UtcNow.AddMonths(-2)).Handle();
        await act.Should().ThrowAsync<DomainException>().Where(ex => ex.DomainErrors.First().ErrorMessage.Contains("Pause date cannot be on or after the end date"));
        _fixture.VerifyLearningPausedEventIsNotPublished();
    }

    [Test]
    public async Task ThenThrowsDomainException_WhenPauseDateIsOnLearningEndDate()
    {
        var act = async () => await _fixture.SetEndDate(DateTime.UtcNow.AddMonths(3)).Handle();
        await act.Should().ThrowAsync<DomainException>().Where(ex => ex.DomainErrors.First().ErrorMessage.Contains("Pause date cannot be on or after the end date"));
        _fixture.VerifyLearningPausedEventIsNotPublished();
    }    

    [Test]
    [TestCase(PaymentStatus.Withdrawn)]
    [TestCase(PaymentStatus.Completed)]
    public async Task ThenThrowsDomainException_WhenStatusIsWithdrawn(PaymentStatus status)
    {
        var act = async () => await _fixture.SetPaymentStatus(status).Handle();
        await act.Should().ThrowAsync<DomainException>().Where(ex => ex.DomainErrors.First().ErrorMessage.Contains("Learning cannot be Paused if Payment Status is Completed or Withdrawn"));
        _fixture.VerifyLearningPausedEventIsNotPublished();
    }
}

public class LearningPausedEventHandlerTestsFixture
{
    public Fixture fixture { get; set; }
    private ProviderCommitmentsDbContext _dbContext;
    private Mock<ILogger<LearningPausedEventHandler>> _mockLogger;
    private Mock<IMessageHandlerContext> _mockContext;
    private LearningPausedEventHandler _handler;
    private LearningPausedEvent _event;
    private Mock<IEncodingService> _mockEncodingService;
    private CommitmentsV2Configuration _commitmentsV2Configuration;
    public UnitOfWorkContext UnitOfWorkContext { get; set; }
    public long apprenticeshipId { get; set; }

    public LearningPausedEventHandlerTestsFixture()
    {
        fixture = new Fixture();
        _mockLogger = new Mock<ILogger<LearningPausedEventHandler>>();
        _mockContext = new Mock<IMessageHandlerContext>();
        _mockEncodingService = new Mock<IEncodingService>();
        _mockEncodingService.Setup(x => x.Encode(It.IsAny<long>(), EncodingType.ApprenticeshipId)).Returns("APP123");
        _mockEncodingService.Setup(x => x.Encode(It.IsAny<long>(), EncodingType.AccountId)).Returns("ACC123");

        UnitOfWorkContext = new UnitOfWorkContext();
        _commitmentsV2Configuration = new CommitmentsV2Configuration
        {
            EmployerCommitmentsBaseUrl = "https://test.com/"
        };

        _event = fixture.Create<LearningPausedEvent>();
      
        _dbContext = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                                            .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
                                            .Options);

        apprenticeshipId = fixture.Create<long>();
        _event.ApprenticeshipId = apprenticeshipId;
        _event.PauseDate = DateTime.UtcNow.AddMonths(3);

        var provider = new Provider()
        {
            UkPrn = 12345,
            Name = "Test Provider"
        };

        var cohort = new Cohort
        {
            Id = fixture.Create<long>(),
            WithParty = Party.Provider,
            Reference = fixture.Create<string>(),
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
            Uln = fixture.Create<long>().ToString(),
            Cohort = cohort,
            StartDate = DateTime.UtcNow.AddMonths(1),
            EndDate = DateTime.UtcNow.AddMonths(13)
        };

        _dbContext.Cohorts.Add(cohort);
        _dbContext.Apprenticeships.Add(Apprenticeship);
        _dbContext.SaveChanges();

        _handler = new LearningPausedEventHandler(new Lazy<ProviderCommitmentsDbContext>(() => _dbContext),
            _mockEncodingService.Object,
            _commitmentsV2Configuration,
            _mockLogger.Object);
    }

    public LearningPausedEventHandlerTestsFixture SetStartDate(DateTime startDate)
    {
        var apprenticeship = _dbContext.Apprenticeships.Find(apprenticeshipId);
        apprenticeship.StartDate = startDate;
        _dbContext.SaveChanges();
        return this;
    }    

    public LearningPausedEventHandlerTestsFixture SetEndDate(DateTime endDate)
    {
        var apprenticeship = _dbContext.Apprenticeships.Find(apprenticeshipId);
        apprenticeship.EndDate = endDate;
        _dbContext.SaveChanges();
        return this;
    }

    public LearningPausedEventHandlerTestsFixture SetPaymentStatus(PaymentStatus status)
    {
        var apprenticeship = _dbContext.Apprenticeships.Find(apprenticeshipId);
        apprenticeship.PaymentStatus = status;
        _dbContext.SaveChanges();
        return this;
    }

    public LearningPausedEventHandlerTestsFixture SetEventApprenticeshipId(long id)
    { 
        _event.ApprenticeshipId = id;
        return this;
    }

    public async Task Handle()
    {
        await _handler.Handle(_event, _mockContext.Object);
    }    

    public void VerifyStoreLearnerHistoryCommandIsSent()
    {
        _mockContext.Verify(x => x.Send(It.Is<StoreLearningHistoryCommand>(c =>
            c.ApprenticeshipId == _event.ApprenticeshipId &&
            c.Source == Types.LearningSourceType.ILRStatusChange &&
            c.ChangeType == Types.LearningChangeType.AutoApproved &&
            c.LearningKey == _event.LearningKey &&
            c.AppliedDate == _event.Created &&
            c.Description == $"Learning has been paused on {_event.PauseDate.ToGdsFormat()}"
        ), It.IsAny<SendOptions>()), Times.Once);
    }

    public void VerifySendEmailToEmployerCommandIsSent()
    {
        var apprenticeship = _dbContext.Apprenticeships.Find(apprenticeshipId);

        _mockContext.Verify(x => x.Send(It.Is<SendEmailToEmployerCommand>(c =>
            c.AccountId == apprenticeship.Cohort.EmployerAccountId &&
            c.Template == "EmployerApprenticeshipPausedNotification" &&
            c.Tokens["provider_name"] == apprenticeship.Cohort.Provider.Name &&
            c.Tokens["link_to_manage_apprenticeships"].Contains(_commitmentsV2Configuration.EmployerCommitmentsBaseUrl) &&
            c.Tokens["link_to_manage_apprenticeships"].Contains("ACC123/apprentices") &&
            c.Tokens["link_to_manage_apprenticeships"].Contains("apprentices/APP123")
        ), It.IsAny<SendOptions>()), Times.Once);
    }

    public void VerifyLearningPausedEventIsPublished()
    {
        _ = _dbContext.Apprenticeships.Find(_event.ApprenticeshipId);
        var pausedEvent = UnitOfWorkContext.GetEvents().OfType<ApprenticeshipPausedEvent>().First();
        pausedEvent.Should().NotBeNull();
        pausedEvent.PausedOn.Date.Should().Be(_event.PauseDate.Date);
        pausedEvent.ApprenticeshipId.Should().Be(_event.ApprenticeshipId);
        pausedEvent.PausedViaILR.Should().BeTrue();
    }

    public void VerifyLearningPausedEventIsNotPublished()
    {
        var pausedEvent = UnitOfWorkContext.GetEvents().OfType<ApprenticeshipPausedEvent>().FirstOrDefault();
        pausedEvent.Should().BeNull();
    }

    public void VerifyLearnerPaused()
    {
        var updatedApprenticeship = _dbContext.Apprenticeships.Find(apprenticeshipId);
        updatedApprenticeship.Should().NotBeNull();
        updatedApprenticeship.PaymentStatus.Should().Be(PaymentStatus.Paused);
        updatedApprenticeship.PauseDate.Should().Be(_event.PauseDate.Date);
    }

    public void Dispose() => _dbContext?.Dispose();
}