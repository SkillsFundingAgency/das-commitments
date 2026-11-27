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
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.ExternalHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Learning.Types;
using SFA.DAS.UnitOfWork.Context;
using TrainingProgramme = SFA.DAS.CommitmentsV2.Domain.Entities.TrainingProgramme;

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
    public async Task Handle_WhenStartDateChangesWithMultiplePriceHistory_LeavesPriceHistoryFromDatesUnchanged()
    {
        await _fixture
            .SetEventWithStartDate("2026-09-15", "2026-10-15")
            .SetPriceHistory(new DateTime(2026, 9, 1), new DateTime(2026, 10, 1))
            .Handle();

        _fixture.GetPriceHistory().Select(price => price.FromDate)
            .Should().BeEquivalentTo([new DateTime(2026, 9, 1), new DateTime(2026, 10, 1)]);
    }

    [Test]
    public async Task Handle_WhenStartDateAndPriceChangeAreApproved_UpdatesTheInitialPriceEpisode()
    {
        await _fixture
            .SetEventWithStartDateAndPrices("2026-09-15", "2026-10-15", new DateTime(2026, 10, 2), 14000, 1000)
            .SetPriceHistory(new DateTime(2026, 9, 1))
            .Handle();

        _fixture.GetPriceHistory().Should().ContainSingle()
            .Which.Should().Match<PriceHistory>(price =>
                price.FromDate == new DateTime(2026, 10, 1) &&
                price.ToDate == null &&
                price.TrainingPrice == 14000 &&
                price.AssessmentPrice == 1000 &&
                price.Cost == 15000);
    }

    [Test]
    public async Task Handle_WhenPriceChangeHasMultipleEpisodes_AppliesTheLatestEpisode()
    {
        await _fixture
            .SetEventWithPrices(
                new PriceEpisode { EffectiveFrom = new DateTime(2026, 9, 1), TrainingPrice = 12000, AssessmentPrice = 3000 },
                new PriceEpisode { EffectiveFrom = new DateTime(2026, 10, 1), TrainingPrice = 14000, AssessmentPrice = 1000 })
            .SetPriceHistory(new DateTime(2026, 9, 1))
            .Handle();

        _fixture.GetPriceHistory().Should().HaveCount(2);
        _fixture.GetPriceHistory().Should().Contain(price =>
            price.FromDate == new DateTime(2026, 9, 1) &&
            price.ToDate == new DateTime(2026, 9, 30));
        _fixture.GetPriceHistory().Should().Contain(price =>
            price.FromDate == new DateTime(2026, 10, 1) &&
            price.TrainingPrice == 14000 &&
            price.AssessmentPrice == 1000 &&
            price.Cost == 15000);
    }

    [Test]
    public async Task Handle_WhenPriceChangeIsForALaterMonth_ClosesTheExistingEpisodeAndAddsANewEpisode()
    {
        await _fixture
            .SetEventWithPrices(new PriceEpisode
            {
                EffectiveFrom = new DateTime(2026, 11, 1),
                TrainingPrice = 14000,
                AssessmentPrice = 1000
            })
            .SetPriceHistory(new DateTime(2026, 9, 1))
            .Handle();

        _fixture.GetPriceHistory().Should().HaveCount(2);
        _fixture.GetPriceHistory().Should().Contain(price =>
            price.FromDate == new DateTime(2026, 9, 1) &&
            price.ToDate == new DateTime(2026, 10, 31));
        _fixture.GetPriceHistory().Should().Contain(price =>
            price.FromDate == new DateTime(2026, 11, 1) &&
            price.ToDate == null &&
            price.Cost == 15000);
    }

    [Test]
    public async Task Handle_WhenPriceScheduleIsEmpty_LeavesPriceHistoryUnchanged()
    {
        await _fixture
            .SetEventWithPrices()
            .SetPriceHistory(new DateTime(2026, 9, 1))
            .Handle();

        _fixture.GetPriceHistory().Should().ContainSingle()
            .Which.Should().Match<PriceHistory>(price =>
                price.FromDate == new DateTime(2026, 9, 1) &&
                price.Cost == 5500);
    }

    [Test]
    public async Task Handle_WhenTheSamePriceEventIsReplayed_DoesNotCreateAnotherEpisode()
    {
        await _fixture
            .SetEventWithPrices(new PriceEpisode
            {
                EffectiveFrom = new DateTime(2026, 10, 1),
                TrainingPrice = 14000,
                AssessmentPrice = 1000
            })
            .SetPriceHistory(new DateTime(2026, 9, 1))
            .Handle();

        await _fixture.Handle();

        _fixture.GetPriceHistory().Should().HaveCount(2);
        _fixture.GetPriceHistory().Should().Contain(price =>
            price.FromDate == new DateTime(2026, 9, 1) &&
            price.ToDate == new DateTime(2026, 9, 30));
        _fixture.GetPriceHistory().Should().Contain(price =>
            price.FromDate == new DateTime(2026, 10, 1) &&
            price.Cost == 15000);
    }

    [Test]
    public async Task Handle_WhenPricesIncludeNewStartWithoutStartDateChange_DoesNotUpdateApprenticeshipStartDate()
    {
        var originalStartDate = new DateTime(2026, 9, 1);
        var originalActualStartDate = new DateTime(2026, 9, 15);

        await _fixture
            .SetApprenticeshipDates(originalStartDate, originalActualStartDate)
            .SetEventWithPrices(new PriceEpisode
            {
                EffectiveFrom = new DateTime(2026, 10, 15),
                TrainingPrice = 14000,
                AssessmentPrice = 1000
            })
            .SetPriceHistory(new DateTime(2026, 9, 1))
            .Handle();

        var apprenticeship = _fixture.GetApprenticeship();
        apprenticeship.StartDate.Should().Be(originalStartDate);
        apprenticeship.ActualStartDate.Should().Be(originalActualStartDate);
        _fixture.GetPriceHistory().Should().HaveCount(2);
        _fixture.GetPriceHistory().Should().Contain(price =>
            price.FromDate == new DateTime(2026, 10, 15) &&
            price.TrainingPrice == 14000 &&
            price.AssessmentPrice == 1000 &&
            price.Cost == 15000);
    }

    [TestCase(LearningType.Apprenticeship, "2026-10-15", "2026-11-20", "1.0", "1.1")]
    [TestCase(LearningType.ApprenticeshipUnit, "2026-10-15", "2026-11-20", "1.0", "1.1")]
    [TestCase(LearningType.ApprenticeshipUnit, "2026-11-14", "2026-11-15", "1.0", "1.1")]
    [TestCase(LearningType.Apprenticeship, "2026-11-20", "2026-10-15", "1.1", "1.0")]
    [TestCase(LearningType.ApprenticeshipUnit, "2026-11-20", "2026-10-15", "1.1", "1.0")]
    [TestCase(LearningType.Apprenticeship, "2026-08-15", "2026-11-20", "1.0", "1.1")]
    [TestCase(LearningType.ApprenticeshipUnit, "2026-08-15", "2026-11-20", "1.0", "1.1")]
    [TestCase(LearningType.Apprenticeship, "2026-10-15", "2026-10-20", "1.0", "1.0")]
    [TestCase(LearningType.ApprenticeshipUnit, "2026-10-15", "2026-10-20", "1.0", "1.0")]
    public async Task Handle_WhenApprovedDateLeavesApprenticeWaitingToStart_UsesVersionForTheNewDate(
        LearningType learningType, string oldDate, string newDate, string previousVersion, string resolvedVersion)
    {
        var apprenticeship = _fixture.SetCourse(learningType, oldDate, previousVersion);
        var courseCode = apprenticeship.CourseCode;
        var originalStandardUId = apprenticeship.StandardUId;
        var originalOption = apprenticeship.TrainingCourseOption;
        _fixture.SetAvailableCourseVersion(learningType, newDate, resolvedVersion);

        await _fixture.SetEventWithStartDate(oldDate, newDate, learningType).Handle();

        var versionChanged = resolvedVersion != previousVersion;
        apprenticeship.ActualStartDate.Should().Be(_fixture.ParseDate(newDate));
        apprenticeship.StartDate.Should().Be(_fixture.ParseFirstDayOfMonth(_fixture.ParseDate(newDate)));
        apprenticeship.GetApprenticeshipStatus(ApprovedLearningUpdatedEventHandlerTestsFixture.Today)
            .Should().Be(ApprenticeshipStatus.WaitingToStart);
        apprenticeship.CourseCode.Should().Be(courseCode);
        apprenticeship.TrainingCourseVersion.Should().Be(resolvedVersion);
        apprenticeship.StandardUId.Should().Be(versionChanged ? $"resolved_{resolvedVersion}" : originalStandardUId);
        apprenticeship.TrainingCourseOption.Should().Be(versionChanged ? null : originalOption);
        _fixture.VerifyNoMessagesSent();
    }

    [TestCase(LearningType.Apprenticeship)]
    [TestCase(LearningType.ApprenticeshipUnit)]
    public async Task Handle_WhenApprovedDateStillLeavesApprenticeStarted_KeepsVersionAndOption(LearningType learningType)
    {
        var apprenticeship = _fixture.SetCourse(learningType, "2026-07-15", "1.0");

        await _fixture.SetEventWithStartDate("2026-07-15", "2026-08-20", learningType).Handle();

        apprenticeship.ActualStartDate.Should().Be(new DateTime(2026, 8, 20));
        apprenticeship.TrainingCourseVersion.Should().Be("1.0");
        apprenticeship.StandardUId.Should().Be("original_1.0");
        apprenticeship.TrainingCourseOption.Should().Be("Existing option");
        _fixture.TrainingProgrammeLookup.VerifyNoOtherCalls();
        _fixture.ApprovalsOuterApiClient.VerifyNoOtherCalls();
    }

    [TestCase(LearningType.Apprenticeship)]
    [TestCase(LearningType.ApprenticeshipUnit)]
    public async Task Handle_WhenThereIsNoVersionForTheApprovedDate_KeepsVersionAndOption(LearningType learningType)
    {
        var apprenticeship = _fixture.SetCourse(learningType, "2026-10-15", "1.0");
        if (learningType == LearningType.Apprenticeship)
        {
            _fixture.SetAvailableCourseVersion(learningType, "2026-11-20", "1.1", new DateTime(2026, 12, 1));
        }

        await _fixture.SetEventWithStartDate("2026-10-15", "2026-11-20", learningType).Handle();

        apprenticeship.ActualStartDate.Should().Be(new DateTime(2026, 11, 20));
        apprenticeship.TrainingCourseVersion.Should().Be("1.0");
        apprenticeship.StandardUId.Should().Be("original_1.0");
        apprenticeship.TrainingCourseOption.Should().Be("Existing option");
    }
}

public class ApprovedLearningUpdatedEventHandlerTestsFixture
{
    public static readonly DateTime Today = new(2026, 9, 10);

    public Fixture fixture { get; set; }
    private ProviderCommitmentsDbContext _dbContext;
    private readonly Mock<ILogger<ApprovedLearningUpdatedEventHandler>> _mockLogger;
    private ApprovedLearningUpdatedEventHandler _handler;
    private ApprovedLearningUpdatedEvent _event;
    private Mock<IMessageHandlerContext> _mockContext;
    public UnitOfWorkContext UnitOfWorkContext { get; set; }
    public long apprenticeshipId { get; set; }
    public Mock<ITrainingProgrammeLookup> TrainingProgrammeLookup { get; } = new();
    public Mock<IApprovalsOuterApiClient> ApprovalsOuterApiClient { get; } = new();

    public string format { get; set; } = "yyyy-MM-dd";

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
            TrainingProgrammeLookup.Object, ApprovalsOuterApiClient.Object,
            Mock.Of<ICurrentDateTime>(clock => clock.UtcNow == Today), _mockLogger.Object);
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

    public ApprovedLearningUpdatedEventHandlerTestsFixture SetEventWithStartDate(
        string oldDate, string newDate, LearningType? learningType = null)
    {
        _event = new ApprovedLearningUpdatedEvent
        {
            ApprenticeshipId = apprenticeshipId,
            LearningKey = Guid.NewGuid(),
            LearningType = learningType?.ToString(),
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

    public ApprovedLearningUpdatedEventHandlerTestsFixture SetEventWithStartDateAndPrices(
        string oldDate,
        string newDate,
        DateTime effectiveFrom,
        decimal trainingPrice,
        decimal assessmentPrice)
    {
        SetEventWithStartDate(oldDate, newDate);
        _event.Prices =
        [
            new()
            {
                EffectiveFrom = effectiveFrom,
                TrainingPrice = trainingPrice,
                AssessmentPrice = assessmentPrice
            }
        ];
        return this;
    }

    public ApprovedLearningUpdatedEventHandlerTestsFixture SetEventWithPrices(params PriceEpisode[] prices)
    {
        _event = new ApprovedLearningUpdatedEvent
        {
            ApprenticeshipId = apprenticeshipId,
            LearningKey = Guid.NewGuid(),
            Prices = prices.ToList()
        };
        return this;
    }

    public Apprenticeship SetCourse(LearningType learningType, string oldDate, string version)
    {
        var apprenticeship = _dbContext.Apprenticeships.Find(apprenticeshipId);
        apprenticeship.PaymentStatus = PaymentStatus.Active;
        apprenticeship.ActualStartDate = ParseDate(oldDate);
        apprenticeship.StartDate = ParseFirstDayOfMonth(apprenticeship.ActualStartDate);
        apprenticeship.CourseCode = learningType == LearningType.ApprenticeshipUnit ? "ZSC00001" : "123";
        apprenticeship.TrainingCourseVersion = version;
        apprenticeship.StandardUId = $"original_{version}";
        apprenticeship.TrainingCourseOption = "Existing option";
        return apprenticeship;
    }

    public void SetAvailableCourseVersion(
        LearningType learningType, string newDate, string version, DateTime? earliestStartDate = null)
    {
        var courseCode = _dbContext.Apprenticeships.Find(apprenticeshipId).CourseCode;
        var standardUId = $"resolved_{version}";
        if (learningType == LearningType.ApprenticeshipUnit)
        {
            var url = $"trainingcourses/apprenticeship-units/{courseCode}/version?startDate={newDate}";
            ApprovalsOuterApiClient
                .Setup(client => client.Get<GetApprenticeshipUnitVersionResponse>(It.Is<IGetApiRequest>(request => request.GetUrl == url)))
                .ReturnsAsync(new GetApprenticeshipUnitVersionResponse { Version = version, StandardUId = standardUId });
            return;
        }

        TrainingProgrammeLookup
            .Setup(lookup => lookup.GetCalculatedTrainingProgrammeVersion(courseCode, ParseDate(newDate).Value))
            .ReturnsAsync(new TrainingProgramme(courseCode, "Test course", version, standardUId, ProgrammeType.Standard, null, null, [], 3)
            {
                VersionEarliestStartDate = earliestStartDate
            });
    }

    public void VerifyNoMessagesSent() => _mockContext.VerifyNoOtherCalls();

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

    public ApprovedLearningUpdatedEventHandlerTestsFixture SetApprenticeshipDates(DateTime startDate, DateTime actualStartDate)
    {
        var apprenticeship = _dbContext.Apprenticeships.Find(apprenticeshipId);
        apprenticeship.StartDate = startDate;
        apprenticeship.ActualStartDate = actualStartDate;
        _dbContext.SaveChanges();
        return this;
    }

    public Apprenticeship GetApprenticeship()
    {
        return _dbContext.Apprenticeships.Find(apprenticeshipId);
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
