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
    private ApprovedLearningUpdatedEventHandlerPriceScheduleTestsFixture _fixture;

    [SetUp]
    public void Arrange()
    {
        _fixture = new ApprovedLearningUpdatedEventHandlerPriceScheduleTestsFixture();
    }

    [TearDown]
    public void TearDown() => _fixture.Dispose();

    [Test]
    public async Task Handle_WhenLatestEpisodeIsNew_ClosesOpenEpisodeAndInsertsOneRow()
    {
        await _fixture
            .WithOpenPriceHistory(from: new DateTime(2024, 8, 1), trainingPrice: 1000m, assessmentPrice: 500m)
            .WithClosedPriceHistory(from: new DateTime(2023, 1, 1), to: new DateTime(2024, 7, 31), cost: 1200m)
            .WithPrices(
                new PriceEpisode { TrainingPrice = 900m, AssessmentPrice = 100m, EffectiveFrom = new DateTime(2024, 8, 1) },
                new PriceEpisode { TrainingPrice = 2000m, AssessmentPrice = 200m, EffectiveFrom = new DateTime(2025, 9, 1) })
            .Handle();

        _fixture.VerifyClosedEpisodeDates(from: new DateTime(2024, 8, 1), to: new DateTime(2025, 8, 31));
        _fixture.VerifyOpenEpisode(from: new DateTime(2025, 9, 1), trainingPrice: 2000m, assessmentPrice: 200m, cost: 2200m);
        _fixture.VerifyHistoricalClosedEpisodeUnchanged(from: new DateTime(2023, 1, 1), to: new DateTime(2024, 7, 31), cost: 1200m);
        _fixture.VerifyPriceHistoryCount(3);
        _fixture.VerifyApprenticeshipPriceFieldsUnchanged();
    }

    [Test]
    public async Task Handle_WhenLatestEpisodeMatchesOpenRow_DoesNotWritePriceHistory_AndStillAppliesOtherChanges()
    {
        await _fixture
            .WithOpenPriceHistory(from: new DateTime(2025, 9, 1), trainingPrice: 2000m, assessmentPrice: 200m)
            .WithApprenticeshipPriceFields(cost: 999m, trainingPrice: 111, assessmentPrice: 222)
            .WithFirstNameChange("Ada")
            .WithPrices(new PriceEpisode { TrainingPrice = 2000m, AssessmentPrice = 200m, EffectiveFrom = new DateTime(2025, 9, 1) })
            .Handle();

        _fixture.VerifyPriceHistoryCount(1);
        _fixture.VerifyOpenEpisode(from: new DateTime(2025, 9, 1), trainingPrice: 2000m, assessmentPrice: 200m, cost: 2200m);
        _fixture.VerifyFirstName("Ada");
        _fixture.VerifyApprenticeshipPriceFields(cost: 999m, trainingPrice: 111, assessmentPrice: 222);
    }

    [Test]
    public async Task Handle_WhenLatestEpisodeMatchesCostOnlyOpenRow_DoesNotWritePriceHistory()
    {
        await _fixture
            .WithOpenCostOnlyPriceHistory(from: new DateTime(2025, 9, 1), cost: 2200m)
            .WithPrices(new PriceEpisode { TrainingPrice = 2000m, AssessmentPrice = 200m, EffectiveFrom = new DateTime(2025, 9, 1) })
            .Handle();

        _fixture.VerifyPriceHistoryCount(1);
        _fixture.VerifyOpenCostOnlyEpisode(from: new DateTime(2025, 9, 1), cost: 2200m);
        _fixture.VerifyApprenticeshipPriceFieldsUnchanged();
    }

    [Test]
    public async Task Handle_WhenSameFromDateDifferentAmounts_RewritesOpenRowInPlace()
    {
        await _fixture
            .WithOpenPriceHistory(from: new DateTime(2025, 9, 1), trainingPrice: 1000m, assessmentPrice: 1100m)
            .WithPrices(new PriceEpisode { TrainingPrice = 1400m, AssessmentPrice = 700m, EffectiveFrom = new DateTime(2025, 9, 1) })
            .Handle();

        _fixture.VerifyPriceHistoryCount(1);
        _fixture.VerifyOpenEpisode(from: new DateTime(2025, 9, 1), trainingPrice: 1400m, assessmentPrice: 700m, cost: 2100m);
        _fixture.VerifyApprenticeshipPriceFieldsUnchanged();
    }

    [Test]
    public async Task Handle_WhenPricesContainsMultipleEpisodes_OnlyLatestIsApplied()
    {
        await _fixture
            .WithOpenPriceHistory(from: new DateTime(2024, 8, 1), trainingPrice: 1000m, assessmentPrice: 500m)
            .WithPrices(
                new PriceEpisode { TrainingPrice = 800m, AssessmentPrice = 200m, EffectiveFrom = new DateTime(2024, 1, 1) },
                new PriceEpisode { TrainingPrice = 900m, AssessmentPrice = 300m, EffectiveFrom = new DateTime(2024, 6, 1) },
                new PriceEpisode { TrainingPrice = 2000m, AssessmentPrice = 200m, EffectiveFrom = new DateTime(2025, 9, 1) })
            .Handle();

        _fixture.VerifyPriceHistoryCount(2);
        _fixture.VerifyClosedEpisodeDates(from: new DateTime(2024, 8, 1), to: new DateTime(2025, 8, 31));
        _fixture.VerifyOpenEpisode(from: new DateTime(2025, 9, 1), trainingPrice: 2000m, assessmentPrice: 200m, cost: 2200m);
        _fixture.VerifyNoPriceHistoryFrom(new DateTime(2024, 1, 1));
        _fixture.VerifyNoPriceHistoryFrom(new DateTime(2024, 6, 1));
    }

    [Test]
    public async Task Handle_WhenPricesAbsentOrEmpty_LeavesPriceHistoryUnchanged()
    {
        await _fixture
            .WithOpenPriceHistory(from: new DateTime(2024, 8, 1), trainingPrice: 1000m, assessmentPrice: 500m)
            .WithEmptyPrices()
            .Handle();

        _fixture.VerifyPriceHistoryCount(1);
        _fixture.VerifyOpenEpisode(from: new DateTime(2024, 8, 1), trainingPrice: 1000m, assessmentPrice: 500m, cost: 1500m);
        _fixture.VerifyApprenticeshipPriceFieldsUnchanged();
    }

    [Test]
    public async Task Handle_WhenPricesNull_LeavesPriceHistoryUnchanged()
    {
        await _fixture
            .WithOpenPriceHistory(from: new DateTime(2024, 8, 1), trainingPrice: 1000m, assessmentPrice: 500m)
            .WithNullPrices()
            .Handle();

        _fixture.VerifyPriceHistoryCount(1);
        _fixture.VerifyOpenEpisode(from: new DateTime(2024, 8, 1), trainingPrice: 1000m, assessmentPrice: 500m, cost: 1500m);
        _fixture.VerifyApprenticeshipPriceFieldsUnchanged();
    }
}

public class ApprovedLearningUpdatedEventHandlerPriceScheduleTestsFixture
{
    public Fixture Fixture { get; }
    public long ApprenticeshipId { get; }

    private readonly ProviderCommitmentsDbContext _dbContext;
    private readonly Mock<ILogger<ApprovedLearningUpdatedEventHandler>> _mockLogger;
    private readonly ApprovedLearningUpdatedEventHandler _handler;
    private readonly Mock<IMessageHandlerContext> _mockContext;
    private ApprovedLearningUpdatedEvent _event;
    private decimal? _originalCost;
    private int? _originalTrainingPrice;
    private int? _originalAssessmentPrice;

    public ApprovedLearningUpdatedEventHandlerPriceScheduleTestsFixture()
    {
        Fixture = new Fixture();
        _mockLogger = new Mock<ILogger<ApprovedLearningUpdatedEventHandler>>();
        _ = new UnitOfWorkContext();
        _mockContext = new Mock<IMessageHandlerContext>();

        _dbContext = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
            .Options);

        ApprenticeshipId = Fixture.Create<long>();

        var provider = new Provider
        {
            UkPrn = 12345,
            Name = "Test Provider"
        };

        var cohort = new Cohort
        {
            Id = Fixture.Create<long>(),
            WithParty = Party.Provider,
            Reference = Fixture.Create<string>(),
            Provider = provider,
            EmployerAccountId = 101
        };

        var apprenticeship = new Apprenticeship
        {
            Id = ApprenticeshipId,
            HasLearnerDataChanges = false,
            FirstName = "Test",
            LastName = "User",
            DateOfBirth = DateTime.UtcNow.AddYears(-20),
            Uln = Fixture.Create<long>().ToString(),
            Cohort = cohort,
            StartDate = DateTime.UtcNow.AddMonths(1),
            EndDate = DateTime.UtcNow.AddMonths(13),
            Cost = 1500m,
            TrainingPrice = 1000,
            EndPointAssessmentPrice = 500
        };

        _originalCost = apprenticeship.Cost;
        _originalTrainingPrice = apprenticeship.TrainingPrice;
        _originalAssessmentPrice = apprenticeship.EndPointAssessmentPrice;

        _dbContext.Cohorts.Add(cohort);
        _dbContext.Apprenticeships.Add(apprenticeship);
        _dbContext.SaveChanges();

        _handler = new ApprovedLearningUpdatedEventHandler(
            new Lazy<ProviderCommitmentsDbContext>(() => _dbContext),
            _mockLogger.Object);

        _event = new ApprovedLearningUpdatedEvent
        {
            ApprenticeshipId = ApprenticeshipId,
            LearningKey = Guid.NewGuid(),
            Changes = [],
            Prices = []
        };
    }

    public ApprovedLearningUpdatedEventHandlerPriceScheduleTestsFixture WithOpenPriceHistory(
        DateTime from,
        decimal trainingPrice,
        decimal assessmentPrice)
    {
        AddPriceHistory(new PriceHistory
        {
            ApprenticeshipId = ApprenticeshipId,
            FromDate = from,
            ToDate = null,
            TrainingPrice = trainingPrice,
            AssessmentPrice = assessmentPrice,
            Cost = trainingPrice + assessmentPrice
        });
        return this;
    }

    public ApprovedLearningUpdatedEventHandlerPriceScheduleTestsFixture WithOpenCostOnlyPriceHistory(DateTime from, decimal cost)
    {
        AddPriceHistory(new PriceHistory
        {
            ApprenticeshipId = ApprenticeshipId,
            FromDate = from,
            ToDate = null,
            TrainingPrice = null,
            AssessmentPrice = null,
            Cost = cost
        });
        return this;
    }

    public ApprovedLearningUpdatedEventHandlerPriceScheduleTestsFixture WithClosedPriceHistory(DateTime from, DateTime to, decimal cost)
    {
        AddPriceHistory(new PriceHistory
        {
            ApprenticeshipId = ApprenticeshipId,
            FromDate = from,
            ToDate = to,
            TrainingPrice = null,
            AssessmentPrice = null,
            Cost = cost
        });
        return this;
    }

    public ApprovedLearningUpdatedEventHandlerPriceScheduleTestsFixture WithApprenticeshipPriceFields(
        decimal cost,
        int trainingPrice,
        int assessmentPrice)
    {
        var apprenticeship = _dbContext.Apprenticeships.Find(ApprenticeshipId);
        apprenticeship.Cost = cost;
        apprenticeship.TrainingPrice = trainingPrice;
        apprenticeship.EndPointAssessmentPrice = assessmentPrice;
        _originalCost = cost;
        _originalTrainingPrice = trainingPrice;
        _originalAssessmentPrice = assessmentPrice;
        _dbContext.SaveChanges();
        return this;
    }

    public ApprovedLearningUpdatedEventHandlerPriceScheduleTestsFixture WithFirstNameChange(string firstName)
    {
        _event.Changes =
        [
            new ApprenticeshipFieldChange
            {
                ChangeType = ApprovedLearnerChangeType.Firstname.ToString(),
                Data = new ApprenticeshipData { Old = "Test", New = firstName }
            }
        ];
        return this;
    }

    public ApprovedLearningUpdatedEventHandlerPriceScheduleTestsFixture WithPrices(params PriceEpisode[] prices)
    {
        _event.Prices = prices.ToList();
        return this;
    }

    public ApprovedLearningUpdatedEventHandlerPriceScheduleTestsFixture WithEmptyPrices()
    {
        _event.Prices = [];
        return this;
    }

    public ApprovedLearningUpdatedEventHandlerPriceScheduleTestsFixture WithNullPrices()
    {
        _event.Prices = null;
        return this;
    }

    public async Task Handle()
    {
        await _handler.Handle(_event, _mockContext.Object);
    }

    public void VerifyPriceHistoryCount(int count)
    {
        GetApprenticeship().PriceHistory.Should().HaveCount(count);
    }

    public void VerifyOpenEpisode(DateTime from, decimal trainingPrice, decimal assessmentPrice, decimal cost)
    {
        var open = GetApprenticeship().PriceHistory.Single(history => history.ToDate == null);
        open.FromDate.Should().Be(from);
        open.TrainingPrice.Should().Be(trainingPrice);
        open.AssessmentPrice.Should().Be(assessmentPrice);
        open.Cost.Should().Be(cost);
    }

    public void VerifyOpenCostOnlyEpisode(DateTime from, decimal cost)
    {
        var open = GetApprenticeship().PriceHistory.Single(history => history.ToDate == null);
        open.FromDate.Should().Be(from);
        open.TrainingPrice.Should().BeNull();
        open.AssessmentPrice.Should().BeNull();
        open.Cost.Should().Be(cost);
    }

    public void VerifyClosedEpisodeDates(DateTime from, DateTime to)
    {
        var closed = GetApprenticeship().PriceHistory.Single(history => history.FromDate == from && history.ToDate != null);
        closed.ToDate.Should().Be(to);
    }

    public void VerifyHistoricalClosedEpisodeUnchanged(DateTime from, DateTime to, decimal cost)
    {
        var historical = GetApprenticeship().PriceHistory.Single(history => history.FromDate == from);
        historical.ToDate.Should().Be(to);
        historical.Cost.Should().Be(cost);
    }

    public void VerifyNoPriceHistoryFrom(DateTime from)
    {
        GetApprenticeship().PriceHistory.Should().NotContain(history => history.FromDate == from);
    }

    public void VerifyFirstName(string firstName)
    {
        GetApprenticeship().FirstName.Should().Be(firstName);
    }

    public void VerifyApprenticeshipPriceFieldsUnchanged()
    {
        VerifyApprenticeshipPriceFields(_originalCost, _originalTrainingPrice, _originalAssessmentPrice);
    }

    public void VerifyApprenticeshipPriceFields(decimal? cost, int? trainingPrice, int? assessmentPrice)
    {
        var apprenticeship = GetApprenticeship();
        apprenticeship.Cost.Should().Be(cost);
        apprenticeship.TrainingPrice.Should().Be(trainingPrice);
        apprenticeship.EndPointAssessmentPrice.Should().Be(assessmentPrice);
    }

    public void Dispose() => _dbContext?.Dispose();

    private void AddPriceHistory(PriceHistory priceHistory)
    {
        var apprenticeship = GetApprenticeship();
        apprenticeship.PriceHistory.Add(priceHistory);
        _dbContext.SaveChanges();
    }

    private Apprenticeship GetApprenticeship()
    {
        return _dbContext.Apprenticeships
            .Include(apprenticeship => apprenticeship.PriceHistory)
            .Single(apprenticeship => apprenticeship.Id == ApprenticeshipId);
    }
}
