using System;
using System.Collections.Generic;
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
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Learning.Types;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.ExternalMessageHandlers.UnitTests.EventHandlers;

public class ApprovedLearningUpdatedEventHandlerPriceScheduleTests
{
    public ApprovedLearningUpdatedEventHandlerPriceScheduleTestsFixture _fixture;

    [SetUp]
    public void Arrange()
    {
        _fixture = new ApprovedLearningUpdatedEventHandlerPriceScheduleTestsFixture();
    }

    [TearDown]
    public void TearDown() => _fixture.Dispose();

    [Test]
    public async Task Handle_WhenApprovedLearningUpdatedEventReceivedWithPriceSchedule_NothingIsUpdated()
    {
        await _fixture.SetEmptyPriceSchedule().Handle();
        _fixture.VerifyPriceScheduleIsUnchanged();
    }

    [Test]
    public async Task Handle_WhenApprovedLearningUpdatedEventReceivedWithPriceSchedule_PriceHistoryUpdated()
    {
        await _fixture.SetEventWithNewPriceSchedule().Handle();
        _fixture.VerifyPriceScheduleIsUpdatedAsExpected();
    }

    [Test]
    public async Task Handle_WhenApprovedLearningUpdatedEventReceivedWithPriceSchedule_PriceUpdatedIsLogged()
    {
        await _fixture.SetEventWithNewPriceSchedule().Handle();
        _fixture.VerifyLoggerInformation($"Processing PriceEpisodes for ApprenticeshipId {_fixture.ApprenticeshipId}");
    }

    [Test]
    public async Task Handle_WhenApprovedLearningUpdatedEventReceivedWithPriceSchedule_CurrentHistoryUpdated()
    {
        await _fixture.SetEventWithNewPriceSchedule().Handle();
        _fixture.VerifyCurrentPriceIsUpdatedAsExpected();
    }
}

public class ApprovedLearningUpdatedEventHandlerPriceScheduleTestsFixture
{
    public Fixture fixture { get; set; }
    private ProviderCommitmentsDbContext _dbContext;
    private readonly Mock<ILogger<ApprovedLearningUpdatedEventHandler>> _mockLogger;
    private ApprovedLearningUpdatedEventHandler _handler;
    private ApprovedLearningUpdatedEvent _event;
    private Mock<IMessageHandlerContext> _mockContext;
    private PriceHistory _priceHistory;
    private List<PriceEpisode> _priceEpisodes;
    public UnitOfWorkContext UnitOfWorkContext { get; set; }
    public long ApprenticeshipId { get; set; }

    public string format { get; set; } = "yyyy-MM-dd";

    public ApprovedLearningUpdatedEventHandlerPriceScheduleTestsFixture()
    {
        fixture = new Fixture();
        _mockLogger = new Mock<ILogger<ApprovedLearningUpdatedEventHandler>>();
        UnitOfWorkContext = new UnitOfWorkContext();
        _mockContext = new Mock<IMessageHandlerContext>();

        _dbContext = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                                            .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
                                            .Options);

        ApprenticeshipId = fixture.Create<long>();
        _priceEpisodes = new List<PriceEpisode>
        {
            new PriceEpisode
            {
                TrainingPrice = 8001,
                AssessmentPrice = 2001,
                EffectiveFrom = DateTime.Today.AddMonths(-2)
            },
            new PriceEpisode
            {
                TrainingPrice = 8000,
                AssessmentPrice = 2000,
                EffectiveFrom = DateTime.Today.AddMonths(-1)
            },
            new PriceEpisode
            {
                TrainingPrice = 9000,
                AssessmentPrice = 1000,
                EffectiveFrom = DateTime.Today
            }
        };


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
            Id = ApprenticeshipId,
            HasLearnerDataChanges = false,
            FirstName = "Test",
            LastName = "User",
            DateOfBirth = DateTime.UtcNow.AddYears(-20),
            Uln = fixture.Create<long>().ToString(),
            Cohort = cohort,
            StartDate = DateTime.UtcNow.AddMonths(1),
            EndDate = DateTime.UtcNow.AddMonths(13)
        };

        _priceHistory = new PriceHistory
        {
            ApprenticeshipId = ApprenticeshipId,
            Cost = 10000,
            TrainingPrice = 8000,
            AssessmentPrice = 2000,
            FromDate = DateTime.UtcNow.AddMonths(-1),
            ToDate = null
        };

        Apprenticeship.PriceHistory.Add(_priceHistory);

        _dbContext.Cohorts.Add(cohort);
        _dbContext.Apprenticeships.Add(Apprenticeship);
        _dbContext.SaveChanges();
        _handler = new ApprovedLearningUpdatedEventHandler(new Lazy<ProviderCommitmentsDbContext>(() => _dbContext),
            _mockLogger.Object);
    }

    public ApprovedLearningUpdatedEventHandlerPriceScheduleTestsFixture SetEventApprenticeshipId(long id)
    {
        _event.ApprenticeshipId = id;
        return this;
    }

    public ApprovedLearningUpdatedEventHandlerPriceScheduleTestsFixture SetEmptyPriceSchedule()
    {
        _event = new ApprovedLearningUpdatedEvent()
        {
            ApprenticeshipId = ApprenticeshipId,
            LearningKey = Guid.NewGuid(),
            Changes = [],
            PriceEpisodes = [],
        };
        return this;
    }

    public ApprovedLearningUpdatedEventHandlerPriceScheduleTestsFixture SetEventWithNewPriceSchedule()
    {
        _event = new ApprovedLearningUpdatedEvent()
        {
            ApprenticeshipId = ApprenticeshipId,
            LearningKey = Guid.NewGuid(),
            Changes = [],
            PriceEpisodes = _priceEpisodes,
        };

        return this;
    }

    public async Task Handle()
    {
        await _handler.Handle(_event, _mockContext.Object);
    }

    public void VerifyPriceScheduleIsUnchanged()
    {
        var updatedApprenticeship = _dbContext.Apprenticeships.Find(ApprenticeshipId);
        updatedApprenticeship.PriceHistory.ToList().Should().BeEquivalentTo(new List<PriceHistory> { _priceHistory });
    }

    public void VerifyPriceScheduleIsUpdatedAsExpected()
    {
        var updatedApprenticeship = _dbContext.Apprenticeships.Find(ApprenticeshipId);
        var list = updatedApprenticeship.PriceHistory.ToList();
        list.Should().HaveCount(3);

        for (var i = 0; i < list.Count; i++)
        {
            list[i].TrainingPrice.Should().Be(_priceEpisodes[i].TrainingPrice);  
            list[i].AssessmentPrice.Should().Be(_priceEpisodes[i].AssessmentPrice);
            list[i].Cost.Should().Be(_priceEpisodes[i].TrainingPrice + _priceEpisodes[i].AssessmentPrice);
            list[i].FromDate.Should().Be(_priceEpisodes[i].EffectiveFrom);
            list[i].ToDate.Should().Be(i < list.Count - 1 ? _priceEpisodes[i + 1].EffectiveFrom.AddDays(-1) : null);
        }
    }

    public void VerifyCurrentPriceIsUpdatedAsExpected()
    {
        var updatedApprenticeship = _dbContext.Apprenticeships.Find(ApprenticeshipId);
        var list = updatedApprenticeship.PriceHistory.ToList();
        list.Should().HaveCount(3);

        updatedApprenticeship.TrainingPrice.Should().Be((int)_priceEpisodes[2].TrainingPrice);
        updatedApprenticeship.EndPointAssessmentPrice.Should().Be((int)_priceEpisodes[2].AssessmentPrice);
        updatedApprenticeship.Cost.Should().Be((int)(_priceEpisodes[2].TrainingPrice + _priceEpisodes[2].AssessmentPrice));
    }

    public void VerifyLoggerInformation(string message)
    {
        _mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
            null,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }

    public void Dispose() => _dbContext?.Dispose();
}