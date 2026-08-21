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
using SFA.DAS.CommitmentsV2.ExternalHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.ExternalHandlers.LearningEvents;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
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
        _fixture.VerifyLoggerWarning("Invalid date for PlannedStartDate change");
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

                   new() { ChangeType = "DOB", Data = new ApprenticeshipData { Old = DateTime.Now.AddYears(-20).ToShortDateString(), New = DateTime.Now.AddYears(-1).ToShortDateString()} },
                   new() { ChangeType = "PlannedStartDate", Data = new ApprenticeshipData { Old = DateTime.Now.AddMonths(2).ToShortDateString(), New = DateTime.Now.AddMonths(4).ToShortDateString()} },
                   new() { ChangeType = "PlannedEndDate", Data = new ApprenticeshipData { Old = DateTime.Now.AddMonths(8).ToShortDateString(), New = DateTime.Now.AddMonths(11).ToShortDateString()} }
            ]
        };
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
                         Old = DateTime.UtcNow.ToShortDateString(),
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
                     ChangeType = "PlannedStartDate",
                     Data = new ApprenticeshipData
                     {
                         Old = DateTime.UtcNow.ToShortDateString(),
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
        updatedApprenticeship.DateOfBirth.Should().Be(ParseDate(GetValue(ApprovedLearnerChangeType.DOB)));
        updatedApprenticeship.StartDate.Should().Be(ParseFirstDayOfMonth(ParseDate(GetValue(ApprovedLearnerChangeType.PlannedStartDate))));
        updatedApprenticeship.EndDate.Should().Be(ParseFirstDayOfMonth(ParseDate(GetValue(ApprovedLearnerChangeType.PlannedEndDate))));
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
        if (DateTime.TryParse(dateString, out var parsedDate))
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