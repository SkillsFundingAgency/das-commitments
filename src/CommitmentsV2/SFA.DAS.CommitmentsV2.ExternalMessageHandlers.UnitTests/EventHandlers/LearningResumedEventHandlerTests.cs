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

public class LearningResumedEventHandlerTests
{
    public LearningResumedEventHandlerTestsFixture _fixture;

    [SetUp]
    public void Arrange()
    {
        _fixture = new LearningResumedEventHandlerTestsFixture();
    }

    [TearDown]
    public void TearDown() => _fixture.Dispose();

    [Test]
    public async Task Handle_WhenLearningResumedEventReceived()
    {
        await _fixture.SetPauseDate(DateTime.UtcNow.AddMonths(1)).SetResumeDate(DateTime.UtcNow.AddMonths(2)).Handle();
        _fixture.VerifyLearnerResumed();
        _fixture.VerifyStoreLearnerHistoryCommandIsSent();
        _fixture.VerifyLearningResumedEventIsPublished();
        _fixture.VerifySendEmailToEmployerCommandIsSent();
    }

    [Test]
    public async Task ThenThrowsDomainException_WhenResumeDateIsEarlierThanLearningStartDate()
    {
        var act = async () => await _fixture.SetStartDate(DateTime.UtcNow.AddMonths(4)).SetResumeDate(DateTime.UtcNow.AddMonths(3)).Handle();
        await act.Should().ThrowAsync<DomainException>().Where(ex => ex.DomainErrors.First().ErrorMessage.Contains("Learner not started"));
        _fixture.VerifyLearningResumedEventIsNotPublished();
    }

    [Test]
    public async Task ThenThrowsDomainException_WhenResumeDateIsOnLearningStartDate()
    {
        var act = async () => await _fixture.SetStartDate(DateTime.UtcNow.AddMonths(3)).SetResumeDate(DateTime.UtcNow.AddMonths(3)).Handle();
        await act.Should().ThrowAsync<DomainException>().Where(ex => ex.DomainErrors.First().ErrorMessage.Contains("Learner not started"));
        _fixture.VerifyLearningResumedEventIsNotPublished();
    }

    [Test]
    public async Task ThenThrowsDomainException_WhenApprenticeshipNotfound()
    {
        var act = async () => await _fixture.SetEventApprenticeshipId(_fixture.fixture.Create<long>()).Handle();
        await act.Should().ThrowAsync<DomainException>().Where(ex => ex.DomainErrors.First().ErrorMessage.Contains("not found"));
        _fixture.VerifyLearningResumedEventIsNotPublished();
    }

    [Test]
    public async Task ThenThrowsDomainException_WhenResumeDateIsAfterLearningEndDate()
    {
        var act = async () => await _fixture.SetEndDate(DateTime.UtcNow.AddMonths(5)).SetResumeDate(DateTime.UtcNow.AddMonths(6)).Handle();
        await act.Should().ThrowAsync<DomainException>().Where(ex => ex.DomainErrors.First().ErrorMessage.Contains("Resume date cannot be on or after the end date"));
        _fixture.VerifyLearningResumedEventIsNotPublished();
    }

    [Test]
    public async Task ThenThrowsDomainException_WhenResumeDateIsOnLearningEndDate()
    {
        var act = async () => await _fixture.SetEndDate(DateTime.UtcNow.AddMonths(3)).SetResumeDate(DateTime.UtcNow.AddMonths(3)).Handle();
        await act.Should().ThrowAsync<DomainException>().Where(ex => ex.DomainErrors.First().ErrorMessage.Contains("Resume date cannot be on or after the end date"));
        _fixture.VerifyLearningResumedEventIsNotPublished();
    }

    [Test]
    public async Task ThenThrowsDomainException_WhenResumeDateIsAfterPauseDate()
    {
        var act = async () => await _fixture.SetPauseDate(DateTime.UtcNow.AddMonths(5)).SetResumeDate(DateTime.UtcNow.AddMonths(4)).Handle();
        await act.Should().ThrowAsync<DomainException>().Where(ex => ex.DomainErrors.First().ErrorMessage.Contains("Resume date cannot be before the pausedate"));
        _fixture.VerifyLearningResumedEventIsNotPublished();
    }

    [Test]
    public async Task Handle_WhenLearningResumedEventReceivedOnPauseDate()
    {
        await _fixture.SetPauseDate(DateTime.UtcNow.AddMonths(2)).SetResumeDate(DateTime.UtcNow.AddMonths(2)).Handle();
        _fixture.VerifyLearnerResumed();
        _fixture.VerifyStoreLearnerHistoryCommandIsSent();
        _fixture.VerifyLearningResumedEventIsPublished();
    }

    [Test]
    public async Task Handle_WhenLearningResumedEventReceivedAndNoPauseDate()
    {
        await _fixture.SetResumeDate(DateTime.UtcNow.AddMonths(2)).Handle();
        _fixture.VerifyLearnerResumed();
        _fixture.VerifyStoreLearnerHistoryCommandIsSent();
        _fixture.VerifyLearningResumedEventIsPublished();
        _fixture.VerifyLoggerLoggedInformation($"Apprenticeship paused date is missing for apprenticeship");
    }

    [Test]
    [TestCase(PaymentStatus.Withdrawn)]
    [TestCase(PaymentStatus.Completed)]
    public async Task ThenThrowsDomainException_WhenStatusIsWithdrawn(PaymentStatus status)
    {
        var act = async () => await _fixture.SetPaymentStatus(status).Handle();
        await act.Should().ThrowAsync<DomainException>().Where(ex => ex.DomainErrors.First().ErrorMessage.Contains("Learning cannot be Resumed if Payment Status is Completed or Withdrawn"));
        _fixture.VerifyLearningResumedEventIsNotPublished();
    }

    [Test]
    public async Task ThenLogstheInformation_WhenStatusIsActive()
    {
        await _fixture.SetPaymentStatus(PaymentStatus.Active).Handle();
        _fixture.VerifyLearningResumedEventIsNotPublished();
        _fixture.VerifyStoreLearningHistoryCommandIsNotSent();
        _fixture.VerifyLoggerLoggedInformation($"Apprenticeship {_fixture.apprenticeshipId} is already active and resumed.");
    }

    [Test]
    public async Task ThenLogstheInformation_WhenEventIsNull()
    {
        await _fixture.SetEventAsNull().Handle();
        _fixture.VerifyLoggerLoggedInformation($"Event received null message : {nameof(LearningResumedEvent)}");
    }
}

public class LearningResumedEventHandlerTestsFixture
{
    public Fixture fixture { get; set; }
    private ProviderCommitmentsDbContext _dbContext;
    private Mock<ILogger<LearningResumedEventHandler>> _mockLogger;
    private Mock<IMessageHandlerContext> _mockContext;
    private LearningResumedEventHandler _handler;
    private LearningResumedEvent _event;
    private Mock<IEncodingService> _mockEncodingService;
    private CommitmentsV2Configuration _commitmentsV2Configuration;
    public UnitOfWorkContext UnitOfWorkContext { get; set; }
    public long apprenticeshipId { get; set; }

    public LearningResumedEventHandlerTestsFixture()
    {
        fixture = new Fixture();
        _mockLogger = new Mock<ILogger<LearningResumedEventHandler>>();
        _mockContext = new Mock<IMessageHandlerContext>();
        _mockEncodingService = new Mock<IEncodingService>();
        _mockEncodingService.Setup(x => x.Encode(It.IsAny<long>(), EncodingType.ApprenticeshipId)).Returns("APP123");
        _mockEncodingService.Setup(x => x.Encode(It.IsAny<long>(), EncodingType.AccountId)).Returns("ACC123");

        UnitOfWorkContext = new UnitOfWorkContext();

        _event = fixture.Create<LearningResumedEvent>();

        _dbContext = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                                            .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
                                            .Options);

        apprenticeshipId = fixture.Create<long>();
        _event.ApprenticeshipId = apprenticeshipId;

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

        _commitmentsV2Configuration = new CommitmentsV2Configuration
        {
            EmployerCommitmentsBaseUrl = "https://test123.com/"
        };

        _dbContext.Cohorts.Add(cohort);
        _dbContext.Apprenticeships.Add(Apprenticeship);
        _dbContext.SaveChanges();

        _handler = new LearningResumedEventHandler(new Lazy<ProviderCommitmentsDbContext>(() => _dbContext), _mockLogger.Object, _mockEncodingService.Object,
            _commitmentsV2Configuration);
    }

    public LearningResumedEventHandlerTestsFixture SetStartDate(DateTime startDate)
    {
        var apprenticeship = _dbContext.Apprenticeships.Find(apprenticeshipId);
        apprenticeship.StartDate = startDate;
        _dbContext.SaveChanges();
        return this;
    }

    public LearningResumedEventHandlerTestsFixture SetEndDate(DateTime endDate)
    {
        var apprenticeship = _dbContext.Apprenticeships.Find(apprenticeshipId);
        apprenticeship.EndDate = endDate;
        _dbContext.SaveChanges();
        return this;
    }

    public LearningResumedEventHandlerTestsFixture SetPauseDate(DateTime pauseDate)
    {
        var apprenticeship = _dbContext.Apprenticeships.Find(apprenticeshipId);
        apprenticeship.PauseDate = pauseDate;
        _dbContext.SaveChanges();
        return this;
    }

    public LearningResumedEventHandlerTestsFixture SetResumeDate(DateTime resumeDate)
    {
        _event.ResumeDate = resumeDate;
        return this;
    }

    public LearningResumedEventHandlerTestsFixture SetPaymentStatus(PaymentStatus status)
    {
        var apprenticeship = _dbContext.Apprenticeships.Find(apprenticeshipId);
        apprenticeship.PaymentStatus = status;
        _dbContext.SaveChanges();
        return this;
    }

    public LearningResumedEventHandlerTestsFixture SetEventApprenticeshipId(long id)
    {
        _event.ApprenticeshipId = id;
        return this;
    }

    public LearningResumedEventHandlerTestsFixture SetEventAsNull()
    {
        _event = null;
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
            c.Description == $"Learning has been resumed on {_event.ResumeDate.ToGdsFormat()}"
        ), It.IsAny<SendOptions>()), Times.Once);
    }

    public void VerifyLearningResumedEventIsPublished()
    {
        _ = _dbContext.Apprenticeships.Find(_event.ApprenticeshipId);
        var resumedEvent = UnitOfWorkContext.GetEvents().OfType<ApprenticeshipResumedEvent>().First();
        resumedEvent.Should().NotBeNull();
        resumedEvent.ResumedOn.Date.Should().Be(_event.ResumeDate.Date);
        resumedEvent.ApprenticeshipId.Should().Be(_event.ApprenticeshipId);
        resumedEvent.ResumedViaILR.Should().BeTrue();
    }

    public void VerifyLearningResumedEventIsNotPublished()
    {
        var resumedEvent = UnitOfWorkContext.GetEvents().OfType<ApprenticeshipResumedEvent>().FirstOrDefault();
        resumedEvent.Should().BeNull();
    }

    public void VerifyStoreLearningHistoryCommandIsNotSent()
    {
        _mockContext.Verify(x => x.Send(It.IsAny<StoreLearningHistoryCommand>(), It.IsAny<SendOptions>()), Times.Never);
    }

    public void VerifyLearnerResumed()
    {
        var updatedApprenticeship = _dbContext.Apprenticeships.Find(apprenticeshipId);
        updatedApprenticeship.Should().NotBeNull();
        updatedApprenticeship.PaymentStatus.Should().Be(PaymentStatus.Active);
        updatedApprenticeship.PauseDate.Should().BeNull();
    }

    public void VerifyLoggerLoggedInformation(string message)
    {
        _mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
            null,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
    }

    public void VerifySendEmailToEmployerCommandIsSent()
    {
        var apprenticeship = _dbContext.Apprenticeships.Find(apprenticeshipId);

        _mockContext.Verify(x => x.Send(It.Is<SendEmailToEmployerCommand>(c =>
            c.AccountId == apprenticeship.Cohort.EmployerAccountId &&
            c.Template == "EmployerApprenticeshipResumedNotification" &&
            c.Tokens["provider_name"] == apprenticeship.Cohort.Provider.Name &&
            c.Tokens["url"].Contains(_commitmentsV2Configuration.EmployerCommitmentsBaseUrl) &&
            c.Tokens["url"].Contains("ACC123/apprentices") &&
            c.Tokens["url"].Contains("apprentices/APP123") && 
            c.NameToken == "name"
        ), It.IsAny<SendOptions>()), Times.Once);
    }

    public void Dispose() => _dbContext?.Dispose();
}