using System;
using System.Collections.Generic;
using System.Globalization;
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
using SFA.DAS.CommitmentsV2.ExternalHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Learning.Types;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.ExternalMessageHandlers.UnitTests.EventHandlers;

public class ApprovedLearningUpdatedEventHandlerTests
{
    public ApprovedLearningUpdatedEventHandlerTestsFixture _fixture;

    [SetUp]
    public void Arrange()
    {
        _fixture = new ApprovedLearningUpdatedEventHandlerTestsFixture();
    }

    [TearDown]
    public void TearDown() => _fixture.Dispose();

    [Test]
    public async Task Handle_WhenApprovedLearningUpdatedEventReceived_ChangeReceived()
    {
        await _fixture.SetEvent().Handle();
        _fixture.VerifyLearnerUpdated();
    }

    [Test]
    public async Task Handle_WhenApprovedLearningUpdatedEventReceived_PublishedApprovedLearningUpdatedEvent()
    {
        await _fixture.SetEvent().Handle();
        _fixture.VerifyLearnerUpdated();
    }


    [Test]
    public async Task ThenLogsWarning_WhenUnknownChangeTypeReceived()
    {
        await _fixture.SetEventWithUnKnownChangeType().Handle();
        _fixture.VerifyLoggerWarning("Unknown change type");
    }

    [Test]
    public async Task ThenLogsMessage_WhenEventIsNull()
    {
        await _fixture.Handle();
        _fixture.VerifyLoggerInformation($"{nameof(ApprovedLearningUpdatedEvent)} received null message");
    }

    [Test]
    public async Task ThenThrowsDomainException_WhenApprenticeshipNotfound()
    {
        var act = async () => await _fixture.SetEvent().SetEventApprenticeshipId(_fixture.fixture.Create<long>()).Handle();
        await act.Should().ThrowAsync<DomainException>().Where(ex => ex.DomainErrors.First().ErrorMessage.Contains("not found"));
    }

    [Test]
    public async Task ThenLogsWarning_WhenInvalidDOBReceived()
    {
        await _fixture.SetEventWithInvalidDOB().Handle();
        _fixture.VerifyLoggerWarning("Invalid date for DOB change");
    }

    [Test]
    public async Task ThenLogsWarning_WhenInvalidStartDateReceived()
    {
        await _fixture.SetEventWithInvalidStartDate().Handle();
        _fixture.VerifyLoggerWarning("Invalid date for StartDate change");
    }

    [Test]
    public async Task Handle_WhenStartDateChangesWithSinglePriceHistory_MovesPriceHistoryFromDateToNewStartDate()
    {
        await _fixture
            .SetEventWithStartDate("2026-09-15", "2026-10-15")
            .SetPriceHistory(new DateTime(2026, 9, 1))
            .Handle();

        _fixture.GetPriceHistory().Should().ContainSingle()
            .Which.FromDate.Should().Be(new DateTime(2026, 10, 1));
    }

    [Test]
    public async Task Handle_WhenApprovedLearningUpdatedEventReceived_ForWithdrawnApprenticeship_ChangeReceivedAndStopDetailsUnchanged()
    {
        _fixture.SetWithdrawnApprenticeship();
        await _fixture.SetEvent().Handle();
        _fixture.VerifyLearnerUpdated();
        _fixture.VerifyWithdrawnStopDetailsUnchanged();
    }

    [Test]
    public async Task Handle_WhenStartDateChangesWithMultiplePriceHistory_LeavesPriceHistoryFromDatesUnchanged()
    {
        await _fixture
            .SetEventWithStartDate("2026-09-15", "2026-10-15")
            .SetPriceHistory(new DateTime(2026, 9, 1), new DateTime(2026, 10, 1))
            .Handle();

        _fixture.GetPriceHistory().Select(price => price.FromDate)
            .Should().BeEquivalentTo([new DateTime(2026, 9, 1), new DateTime(2026, 10, 1)]);
    }
}

public class ApprovedLearningUpdatedEventHandlerTestsFixture
{
    public Fixture fixture { get; set; }
    private ProviderCommitmentsDbContext _dbContext;
    private readonly Mock<ILogger<ApprovedLearningUpdatedEventHandler>> _mockLogger;
    private ApprovedLearningUpdatedEventHandler _handler;
    private ApprovedLearningUpdatedEvent _event;
    private Mock<IMessageHandlerContext> _mockContext;
    public UnitOfWorkContext UnitOfWorkContext { get; set; }
    public long apprenticeshipId { get; set; }

    public string format { get; set; } = "yyyy-MM-dd";

    private readonly DateTime _stopDate = DateTime.UtcNow.AddMonths(-1).Date;
    private const int _withdrawnReasonCode = 1;

    public ApprovedLearningUpdatedEventHandlerTestsFixture()
    {
        fixture = new Fixture();
        _mockLogger = new Mock<ILogger<ApprovedLearningUpdatedEventHandler>>();
        UnitOfWorkContext = new UnitOfWorkContext();
        _mockContext = new Mock<IMessageHandlerContext>();

        _dbContext = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                                            .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
                                            .Options);

        apprenticeshipId = fixture.Create<long>();

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
        _handler = new ApprovedLearningUpdatedEventHandler(new Lazy<ProviderCommitmentsDbContext>(() => _dbContext),
            _mockLogger.Object);
    }

    public ApprovedLearningUpdatedEventHandlerTestsFixture SetWithdrawnApprenticeship()
    {
        var apprenticeship = _dbContext.Apprenticeships.Find(apprenticeshipId);
        apprenticeship.PaymentStatus = PaymentStatus.Withdrawn;
        apprenticeship.StopDate = _stopDate;
        apprenticeship.WithdrawnReasonCode = _withdrawnReasonCode;
        _dbContext.SaveChanges();
        return this;
    }

    public ApprovedLearningUpdatedEventHandlerTestsFixture SetEventApprenticeshipId(long id)
    {
        _event.ApprenticeshipId = id;
        return this;
    }

    public ApprovedLearningUpdatedEventHandlerTestsFixture SetEvent()
    {
        _event = new ApprovedLearningUpdatedEvent()
        {
            ApprenticeshipId = apprenticeshipId,
            LearningKey = Guid.NewGuid(),
            Changes =
             [
                 new() {
                     ChangeType = "Firstname",
                     Data = new ApprenticeshipData
                     {
                         Old = "Test",
                         New = "Updated"
                     }
                 },
                    new() {
                        ChangeType = "Surname",
                        Data = new ApprenticeshipData
                        {
                            Old = "User",
                            New = "Updated"
                        }
                    },
                   new() {
                        ChangeType = "Email",
                        Data = new ApprenticeshipData
                        {
                            Old = "test.user@domain.com",
                            New = "updated.user@domain.com"
                        }
                   },

                   new() { ChangeType = "DOB", Data = new ApprenticeshipData { Old = DateTime.Now.AddYears(-20).ToString(format), New = DateTime.Now.AddYears(-1).ToString(format)} },
                   new() { ChangeType = "StartDate", Data = new ApprenticeshipData { Old = DateTime.Now.AddMonths(2).ToString(format), New = DateTime.Now.AddMonths(4).ToString(format)} },
                   new() { ChangeType = "PlannedEndDate", Data = new ApprenticeshipData { Old = DateTime.Now.AddMonths(8).ToString(format), New = DateTime.Now.AddMonths(11).ToString(format)} }
            ]
        };
        return this;
    }

    public ApprovedLearningUpdatedEventHandlerTestsFixture SetEventWithStartDate(string oldDate, string newDate)
    {
        _event = new ApprovedLearningUpdatedEvent
        {
            ApprenticeshipId = apprenticeshipId,
            LearningKey = Guid.NewGuid(),
            Changes =
            [
                new()
                {
                    ChangeType = nameof(ApprovedLearnerChangeType.StartDate),
                    Data = new ApprenticeshipData
                    {
                        Old = oldDate,
                        New = newDate
                    }
                }
            ]
        };

        return this;
    }

    public ApprovedLearningUpdatedEventHandlerTestsFixture SetPriceHistory(params DateTime[] fromDates)
    {
        var apprenticeship = _dbContext.Apprenticeships.Find(apprenticeshipId);
        foreach (var fromDate in fromDates)
        {
            apprenticeship.PriceHistory.Add(new PriceHistory
            {
                ApprenticeshipId = apprenticeshipId,
                FromDate = fromDate,
                ToDate = null,
                Cost = 5500,
                TrainingPrice = 5000,
                AssessmentPrice = 500
            });
        }

        _dbContext.SaveChanges();
        return this;
    }

    public ApprovedLearningUpdatedEventHandlerTestsFixture SetEventWithUnKnownChangeType()
    {
        _event = new ApprovedLearningUpdatedEvent()
        {
            ApprenticeshipId = apprenticeshipId,
            LearningKey = Guid.NewGuid(),
            Changes =
            [
                new() {
                     ChangeType = "TestName",
                     Data = new ApprenticeshipData
                     {
                         Old = "Test",
                         New = "Updated"
                     }
                 }]
        };

        return this;
    }

    public ApprovedLearningUpdatedEventHandlerTestsFixture SetEventWithInvalidDOB()
    {
        _event = new ApprovedLearningUpdatedEvent()
        {
            ApprenticeshipId = apprenticeshipId,
            LearningKey = Guid.NewGuid(),
            Changes =
            [
                new() {
                     ChangeType = "DOB",
                     Data = new ApprenticeshipData
                     {
                         Old = DateTime.UtcNow.ToString(format),
                         New = "20!6-01-01"
                     }
                 }]
        };

        return this;
    }

    public ApprovedLearningUpdatedEventHandlerTestsFixture SetEventWithInvalidStartDate()
    {
        _event = new ApprovedLearningUpdatedEvent()
        {
            ApprenticeshipId = apprenticeshipId,
            LearningKey = Guid.NewGuid(),
            Changes =
            [
                new() {
                     ChangeType = "StartDate",
                     Data = new ApprenticeshipData
                     {
                         Old = DateTime.UtcNow.ToString(format),
                         New = "20!6-01-01"
                     }
                 }]
        };

        return this;
    }

    public async Task Handle()
    {
        await _handler.Handle(_event, _mockContext.Object);
    }

    public void VerifyLearnerUpdated()
    {
        var updatedApprenticeship = _dbContext.Apprenticeships.Find(apprenticeshipId);
        updatedApprenticeship.Should().NotBeNull();
        updatedApprenticeship.FirstName.Should().Be(GetValue(ApprovedLearnerChangeType.Firstname));
        updatedApprenticeship.LastName.Should().Be(GetValue(ApprovedLearnerChangeType.Surname));
        updatedApprenticeship.Email.Should().Be(GetValue(ApprovedLearnerChangeType.Email));
        updatedApprenticeship.DateOfBirth.Value.Date.Should().Be(ParseDate(GetValue(ApprovedLearnerChangeType.DOB)));
        updatedApprenticeship.StartDate.Should().Be(ParseFirstDayOfMonth(ParseDate(GetValue(ApprovedLearnerChangeType.StartDate))));
        updatedApprenticeship.EndDate.Should().Be(ParseFirstDayOfMonth(ParseDate(GetValue(ApprovedLearnerChangeType.PlannedEndDate))));
        updatedApprenticeship.ActualStartDate.Should().Be(ParseDate(GetValue(ApprovedLearnerChangeType.StartDate)));
    }

    public List<PriceHistory> GetPriceHistory()
    {
        return _dbContext.Apprenticeships
            .Include(apprenticeship => apprenticeship.PriceHistory)
            .Single(apprenticeship => apprenticeship.Id == apprenticeshipId)
            .PriceHistory
            .ToList();
    }

    public void VerifyWithdrawnStopDetailsUnchanged()
    {
        var updatedApprenticeship = _dbContext.Apprenticeships.Find(apprenticeshipId);
        updatedApprenticeship.PaymentStatus.Should().Be(PaymentStatus.Withdrawn);
        updatedApprenticeship.StopDate.Should().Be(_stopDate);
        updatedApprenticeship.WithdrawnReasonCode.Should().Be(_withdrawnReasonCode);
    }

    private string GetValue(ApprovedLearnerChangeType changeType)
    {
        return _event.Changes.Where(t => t.ChangeType == Enum.GetName(changeType)).Select(t => t.Data.New).FirstOrDefault();
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

    public void VerifyLoggerWarning(string message)
    {
        _mockLogger.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
            null,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }

    public void Dispose() => _dbContext?.Dispose();

    public DateTime? ParseDate(string dateString)
    {
        if (DateTime.TryParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
        {
            return parsedDate;
        }
        return null;
    }

    public DateTime? ParseFirstDayOfMonth(DateTime? date)
    {
        return date.HasValue ? new DateTime(date.Value.Year, date.Value.Month, 1) : null;
    }
}