using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services;

[TestFixture]
public class CocApprovalStatusServiceTestsForEndDate
{
    private Mock<ILogger<CocApprovalStatusService>> _loggerMock;
    private Mock<IOverlapCheckService> _overlapCheckServiceMock;
    private CocApprovalStatusService _service;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<CocApprovalStatusService>>();
        _overlapCheckServiceMock = new Mock<IOverlapCheckService>();
        _overlapCheckServiceMock.Setup(x => x.CheckForOverlaps(It.IsAny<string>(), It.IsAny<CourseDateRange>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OverlapCheckResult(false, false));

        _service = new CocApprovalStatusService(_overlapCheckServiceMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task DetermineCocUpdateStatusesForEndDate_ShouldReturnEmpty_WhenNoEndDateFieldPresent()
    {
        var updates = new CocUpdates();
        var apprenticeship = new Apprenticeship { EndDate = DateTime.Today };

        var result = await _service.DetermineCocUpdateStatuses(updates, apprenticeship);

        result.Should().BeEmpty();
    }

    [Test]
    public async Task DetermineCocUpdateStatusesForEndDate_ShouldLogInformation_WhenPlannedEndDateFieldPresent()
    {
        var updates = new CocUpdates
        {
            PlannedEndDate = new CocUpdate<DateTime?> { Old = DateTime.Today, New = DateTime.Today.AddDays(10) }
        };
        var apprenticeship = new Apprenticeship
        {
            EndDate = DateTime.Today,
            StartDate = DateTime.Today.AddMonths(-3),
            PaymentStatus = Types.PaymentStatus.Completed,
            CompletionDate = DateTime.Today.AddDays(-1)
        };

        await _service.DetermineCocUpdateStatuses(updates, apprenticeship);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, _) => o.ToString().Contains("Change of PlannedEndDate detected")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Test]
    public async Task DetermineCocUpdateStatusesForEndDate_ShouldLogInformation_WhenPlannedEndDateOldValueIsNotEqualCurrentValue()
    {
        var updates = new CocUpdates
        {
            PlannedEndDate = new CocUpdate<DateTime?> { Old = DateTime.Today, New = DateTime.Today.AddDays(10) }
        };
        var apprenticeship = new Apprenticeship
        {
            EndDate = DateTime.Today.AddMonths(-1),
            StartDate = DateTime.Today.AddMonths(-3),
            PaymentStatus = Types.PaymentStatus.Completed,
            CompletionDate = DateTime.Today.AddDays(-1)
        };

        await _service.DetermineCocUpdateStatuses(updates, apprenticeship);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, _) => o.ToString().Contains("Old planned end date from changes does not match apprenticeship end date")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Test]
    public async Task DetermineCocUpdateStatusesForEndDate_ShouldReturnAutoRejected_WhenPlannedEndDateIsAfterCompletionDate()
    {
        var updates = new CocUpdates
        {
            PlannedEndDate = new CocUpdate<DateTime?> { Old = DateTime.Today, New = DateTime.Today.AddMonths(10) }
        };
        var apprenticeship = new Apprenticeship
        {
            EndDate = DateTime.Today,
            StartDate = DateTime.Today.AddMonths(-3),
            PaymentStatus = Types.PaymentStatus.Completed,
            CompletionDate = DateTime.Today.AddMonths(1)
        };

        var results = await _service.DetermineCocUpdateStatuses(updates, apprenticeship);

        results.Should().HaveCount(1);
        results[0].Status.Should().Be(CocApprovalItemStatus.AutoRejected);
        results[0].Field.Should().Be(CocChangeField.PlannedEndDate);
        results[0].Reason.Should().Be("The end date cannot be changed on a completed record");
    }

    [Test]
    public async Task DetermineCocUpdateStatusesForEndDate_ShouldReturnAutoRejected_WhenApprenticeshipIsStopped()
    {
        var updates = new CocUpdates
        {
            PlannedEndDate = new CocUpdate<DateTime?> { Old = DateTime.Today, New = DateTime.Today.AddMonths(10) }
        };
        var apprenticeship = new Apprenticeship
        {
            EndDate = DateTime.Today,
            StartDate = DateTime.Today.AddMonths(-3),
            StopDate = DateTime.Today.AddMonths(-1)
        };

        var results = await _service.DetermineCocUpdateStatuses(updates, apprenticeship);

        results.Should().HaveCount(1);
        results[0].Status.Should().Be(CocApprovalItemStatus.AutoRejected);
        results[0].Field.Should().Be(CocChangeField.PlannedEndDate);
        results[0].Reason.Should().Be("The end date cannot be changed on a stopped record");
    }

    [Test]
    public async Task DetermineCocUpdateStatusesForEndDate_ShouldReturnAutoRejected_WhenPlannedEndDateIsBeforeStartOdService()
    {
        var updates = new CocUpdates
        {
            PlannedEndDate = new CocUpdate<DateTime?> { Old = DateTime.Today, New = CommitmentsV2.Domain.Constants.DasStartDate.AddMonths(-1) }
        };
        var apprenticeship = new Apprenticeship
        {
            EndDate = DateTime.Today,
            StartDate = DateTime.Today.AddMonths(-3)
        };

        var results = await _service.DetermineCocUpdateStatuses(updates, apprenticeship);

        results.Should().HaveCount(1);
        results[0].Status.Should().Be(CocApprovalItemStatus.AutoRejected);
        results[0].Field.Should().Be(CocChangeField.PlannedEndDate);
        results[0].Reason.Should().Be("The end date must not be earlier than May 2017");
    }

    [Test]
    public async Task DetermineCocUpdateStatusesForEndDate_ShouldReturnAutoRejected_WhenPlannedEndDateIsBeforeStartDate()
    {
        var updates = new CocUpdates
        {
            PlannedEndDate = new CocUpdate<DateTime?> { Old = DateTime.Today, New = DateTime.Today.AddMonths(-6) }
        };
        var apprenticeship = new Apprenticeship
        {
            EndDate = DateTime.Today,
            StartDate = DateTime.Today.AddMonths(-3)
        };

        var results = await _service.DetermineCocUpdateStatuses(updates, apprenticeship);

        results.Should().HaveCount(1);
        results[0].Status.Should().Be(CocApprovalItemStatus.AutoRejected);
        results[0].Field.Should().Be(CocChangeField.PlannedEndDate);
        results[0].Reason.Should().Be("The end date must not be before the start date");
    }

    [Test]
    public async Task DetermineCocUpdateStatusesForEndDate_ShouldReturnAutoRejected_WhenPlannedEndDateIsBeforeFlexiEmploymentDate()
    {
        var updates = new CocUpdates
        {
            PlannedEndDate = new CocUpdate<DateTime?> { Old = DateTime.Today, New = DateTime.Today.AddMonths(3) }
        };
        var apprenticeship = new Apprenticeship
        {
            EndDate = DateTime.Today,
            StartDate = DateTime.Today.AddMonths(-3),
            FlexibleEmployment = new FlexibleEmployment { EmploymentEndDate = DateTime.Today.AddMonths(6) }
        };

        var results = await _service.DetermineCocUpdateStatuses(updates, apprenticeship);

        results.Should().HaveCount(1);
        results[0].Status.Should().Be(CocApprovalItemStatus.AutoRejected);
        results[0].Field.Should().Be(CocChangeField.PlannedEndDate);
        results[0].Reason.Should().Be("The end date must not be earlier than FlexibleEmployment.EmploymentEndDate");
    }

    [Test]
    public async Task DetermineCocUpdateStatusesForEndDate_ShouldReturnAutoRejected_WhenUlnOverlapOccurs()
    {
        var updates = new CocUpdates
        {
            PlannedEndDate = new CocUpdate<DateTime?> { Old = DateTime.Today, New = DateTime.Today.AddMonths(3) }
        };
        var apprenticeship = new Apprenticeship
        {
            Id = 1234,
            Uln = "1234567890",
            EndDate = DateTime.Today,
            StartDate = DateTime.Today.AddMonths(-3)
        };

        _overlapCheckServiceMock.Setup(x => x.CheckForOverlaps(apprenticeship.Uln, It.IsAny<CourseDateRange>(), apprenticeship.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OverlapCheckResult(true, true));

        var results = await _service.DetermineCocUpdateStatuses(updates, apprenticeship);

        results.Should().HaveCount(1);
        results[0].Status.Should().Be(CocApprovalItemStatus.AutoRejected);
        results[0].Field.Should().Be(CocChangeField.PlannedEndDate);
        results[0].Reason.Should().Be("Apprentices overlapping dates and therefore cant start");
    }

    [Test]
    public async Task DetermineCocUpdateStatusesForEndDate_ShouldReturnPending_WhenNoErrors()
    {
        var updates = new CocUpdates
        {
            PlannedEndDate = new CocUpdate<DateTime?> { Old = DateTime.Today, New = DateTime.Today.AddMonths(3) }
        };
        var apprenticeship = new Apprenticeship
        {
            Id = 1234,
            Uln = "1234567890",
            EndDate = DateTime.Today,
            StartDate = DateTime.Today.AddMonths(-3),
        };

        _overlapCheckServiceMock.Setup(x => x.CheckForOverlaps(apprenticeship.Uln, It.IsAny<CourseDateRange>(), apprenticeship.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OverlapCheckResult(false, false));

        var results = await _service.DetermineCocUpdateStatuses(updates, apprenticeship);

        results.Should().HaveCount(1);
        results[0].Status.Should().Be(CocApprovalItemStatus.Pending);
    }
}