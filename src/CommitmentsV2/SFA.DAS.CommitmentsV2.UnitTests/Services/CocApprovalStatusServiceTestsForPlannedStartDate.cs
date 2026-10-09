using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Shared.Extensions;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Validation.CocApprovals.Interfaces;
using SFA.DAS.CommitmentsV2.Domain;
using SFA.DAS.CommitmentsV2.Domain.Entities.Reservations;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services;

public class CocApprovalStatusServiceTestsForPlannedStartDate
{
    private Mock<ILogger<CocApprovalStatusService>> _loggerMock;
    private Mock<IOverlapCheckService> _overlapCheckServiceMock;
    private Mock<IAcademicYearDateProvider> _academicYearDateProviderMock;
    private Mock<IPlannedStartDateValidationRules> _plannedStartDateValidationRulesMock;
    private CocApprovalStatusService _service;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<CocApprovalStatusService>>();
        _overlapCheckServiceMock = new Mock<IOverlapCheckService>();
        _academicYearDateProviderMock = new Mock<IAcademicYearDateProvider>();
        _plannedStartDateValidationRulesMock = new Mock<IPlannedStartDateValidationRules>();
        _service = new CocApprovalStatusService(_overlapCheckServiceMock.Object, _plannedStartDateValidationRulesMock.Object, _academicYearDateProviderMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task DetermineCocUpdateStatusesAsync_ShouldAutoApprovePlannedStartDate_WhenAllValidationRulesPass()
    {
        var plannedStartDate = new DateTime(2027, 1, 1);

        _plannedStartDateValidationRulesMock.Setup(x => x.IsPlannedStartDateValidForReservationAsync(plannedStartDate, It.IsAny<Apprenticeship>())).ReturnsAsync((ReservationValidationResult)null);

        var approvalDetails = CreatePlannedStartDateApprovalDetails(plannedStartDate);

        var result = await _service.DetermineCocUpdateStatusesAsync(approvalDetails.Updates, approvalDetails.Apprenticeship, approvalDetails.Course);

        result.Should().ContainSingle(x => x.Field == CocChangeField.PlannedStartDate && x.Status == CocApprovalItemStatus.AutoApproved);
    }

    [Test]
    public async Task DetermineCocUpdateStatusesAsync_ShouldAutoReject_WhenPlannedStartDateBeforeAbsoluteMinimum()
    {
        var plannedStartDate = new DateTime(2016, 1, 1);

        _plannedStartDateValidationRulesMock.Setup(x => x.IsPlannedStartDateBeforeAbsoluteMinimum(plannedStartDate)).Returns(true);

        var approvalDetails = CreatePlannedStartDateApprovalDetails(plannedStartDate);

        var result = await _service.DetermineCocUpdateStatusesAsync(approvalDetails.Updates, approvalDetails.Apprenticeship, approvalDetails.Course);

        result.Should().HaveCount(1);
        result[0].Field.Should().Be(CocChangeField.PlannedStartDate);
        result[0].Status.Should().Be(CocApprovalItemStatus.AutoRejected);
        result[0].Reason.Should().Be("The start date must not be earlier than May 2017");
    }

    [Test]
    public async Task DetermineCocUpdateStatusesAsync_ShouldAutoReject_WhenPlannedStartDateAfterMaximum()
    {
        var plannedStartDate = new DateTime(2030, 1, 1);

        _plannedStartDateValidationRulesMock.Setup(x => x.IsPlannedStartDateAfterMaximum(plannedStartDate)).Returns(true);

        var approvalDetails = CreatePlannedStartDateApprovalDetails(plannedStartDate);

        var result = await _service.DetermineCocUpdateStatusesAsync(approvalDetails.Updates, approvalDetails.Apprenticeship, approvalDetails.Course);

        result.Should().HaveCount(1);
        result[0].Status.Should().Be(CocApprovalItemStatus.AutoRejected);
        result[0].Reason.Should().Be("The start date must be no later than one year after the end of the current teaching year");
    }

    [Test]
    public async Task DetermineCocUpdateStatusesAsync_ShouldAutoReject_WhenPlannedStartDateBeforeFundingWindowHasClosed()
    {
        var plannedStartDate = new DateTime(2024, 1, 1);
        var academicYearStart = new DateTime(2025, 8, 1);

        _plannedStartDateValidationRulesMock.Setup(x => x.IsPlannedStartDateBeforeFundingWindowHasClosed(plannedStartDate)).Returns(true);

        _academicYearDateProviderMock.Setup(x => x.CurrentAcademicYearStartDate).Returns(academicYearStart);

        var approvalDetails = CreatePlannedStartDateApprovalDetails(plannedStartDate);

        var result = await _service.DetermineCocUpdateStatusesAsync(approvalDetails.Updates, approvalDetails.Apprenticeship, approvalDetails.Course);

        result.Should().HaveCount(1);
        result[0].Status.Should().Be(CocApprovalItemStatus.AutoRejected);
        result[0].Reason.Should().Be($"The earliest start date you can use is {academicYearStart.ToGdsFormatShortMonthWithoutDay()}");
    }

    [Test]
    public async Task DetermineCocUpdateStatusesAsync_ShouldAutoReject_WhenPlannedStartDateBeforeLarsEffectiveFrom()
    {
        var plannedStartDate = new DateTime(2025, 1, 1);
        var effectiveFrom = new DateTime(2025, 5, 1);

        _plannedStartDateValidationRulesMock.Setup(x => x.IsPlannedStartDateBeforeLarsEffectiveFrom(plannedStartDate, It.IsAny<Course>())).Returns(true);

        var approvalDetails = CreatePlannedStartDateApprovalDetails(plannedStartDate);
        approvalDetails.Course.EffectiveFrom = effectiveFrom;

        var result = await _service.DetermineCocUpdateStatusesAsync(approvalDetails.Updates, approvalDetails.Apprenticeship, approvalDetails.Course);

        result.Should().HaveCount(1);
        result[0].Status.Should().Be(CocApprovalItemStatus.AutoRejected);
        result[0].Reason.Should().Be("This training course is only available to learners with a start date after 4 2025");
    }

    [Test]
    public async Task DetermineCocUpdateStatusesAync_ShouldAutoReject_WhenPlannedStartDateAfterLarsEffectiveTo()
    {
        var plannedStartDate = new DateTime(2025, 1, 1);
        var effectiveTo = new DateTime(2025, 5, 1);

        _plannedStartDateValidationRulesMock.Setup(x => x.IsPlannedStartDateAfterLarsEffectiveTo(plannedStartDate, It.IsAny<Course>())).Returns(true);

        var approvalDetails = CreatePlannedStartDateApprovalDetails(plannedStartDate);
        approvalDetails.Course.EffectiveTo = effectiveTo;

        var result = await _service.DetermineCocUpdateStatusesAsync(approvalDetails.Updates, approvalDetails.Apprenticeship, approvalDetails.Course);

        result.Should().HaveCount(1);
        result[0].Status.Should().Be(CocApprovalItemStatus.AutoRejected);
        result[0].Reason.Should().Be("This training course is only available to learners with a start date before 6 2025");
    }

    [Test]
    public async Task DetermineCocUpdateStatusesAsync_ShouldAutoReject_WhenPlannedStartDateBeforeTransferFundedDate()
    {
        var plannedStartDate = new DateTime(2017, 1, 1);

        _plannedStartDateValidationRulesMock.Setup(x => x.IsPlannedStartDateBeforeTransferFundedDate(plannedStartDate, It.IsAny<Cohort>())).Returns(true);

        var approvalDetails = CreatePlannedStartDateApprovalDetails(plannedStartDate);

        var result = await _service.DetermineCocUpdateStatusesAsync(approvalDetails.Updates, approvalDetails.Apprenticeship, approvalDetails.Course);

        result.Should().HaveCount(1);
        result[0].Status.Should().Be(CocApprovalItemStatus.AutoRejected);
        result[0].Reason.Should().Be("Learners funded through a transfer can't start earlier than May 2018");
    }

    [Test]
    public async Task DetermineCocUpdateStatusesAync_ShouldAutoReject_WhenPlannedStartDateUlnDateRangeOverlaps()
    {
        var plannedStartDate = new DateTime(2026, 1, 1);

        _plannedStartDateValidationRulesMock.Setup(x => x.IsPlannedStartDateOverlappingWithUlnDateRangeAsync(plannedStartDate, It.IsAny<Apprenticeship>())).ReturnsAsync(true);

        var approvalDetails = CreatePlannedStartDateApprovalDetails(plannedStartDate);

        var result = await _service.DetermineCocUpdateStatusesAsync(approvalDetails.Updates, approvalDetails.Apprenticeship, approvalDetails.Course);

        result.Should().HaveCount(1);
        result[0].Status.Should().Be(CocApprovalItemStatus.AutoRejected);
        result[0].Reason.Should().Be("Learners overlapping dates and therefore cant start");
    }

    [Test]
    public async Task DetermineCocUpdateStatusesAsync_ShouldAutoReject_WhenPlannedStartDateReservationValidationFails()
    {
        var plannedStartDate = new DateTime(2026, 1, 1);

        var reservationValidationErrors = new ReservationValidationError[]
        {
            new ReservationValidationError("Property", "Reservation validation failed")
        };

        _plannedStartDateValidationRulesMock.Setup(x => x.IsPlannedStartDateValidForReservationAsync(plannedStartDate, It.IsAny<Apprenticeship>())).ReturnsAsync(new ReservationValidationResult(reservationValidationErrors));

        var approvalDetails = CreatePlannedStartDateApprovalDetails(plannedStartDate);

        var result = await _service.DetermineCocUpdateStatusesAsync(approvalDetails.Updates, approvalDetails.Apprenticeship, approvalDetails.Course);

        result.Should().HaveCount(1);
        result[0].Status.Should().Be(CocApprovalItemStatus.AutoRejected);
        result[0].Reason.Should().Be("Reservation validation failed");
    }

    [Test]
    public async Task DetermineCocUpdateStatusesAsync_ShouldAutoRejectPlannedStartDate_WhenLearnerIsTooYoung()
    {
        var plannedStartDate = new DateTime(2026, 1, 1);

        _plannedStartDateValidationRulesMock.Setup(x => x.IsPlannedStartDateLessThanMinAge(plannedStartDate, It.IsAny<DateTime?>())).Returns(true);

        var approvalDetails = CreatePlannedStartDateApprovalDetails(plannedStartDate);

        var result = await _service.DetermineCocUpdateStatusesAsync(approvalDetails.Updates, approvalDetails.Apprenticeship, approvalDetails.Course);

        result.Should().HaveCount(1);
        result[0].Status.Should().Be(CocApprovalItemStatus.AutoRejected);
        result[0].Reason.Should().Be($"The learner must be at least {Constants.MinimumAgeAtApprenticeshipStart} years old at the start of their training");
    }

    [Test]
    public async Task DetermineCocUpdateStatusesAsync_ShouldAutoRejectPlannedStartDate_WhenLearnerExceedsMaximumAge()
    {
        var plannedStartDate = new DateTime(2026, 1, 1);

        _plannedStartDateValidationRulesMock.Setup(x => x.IsPlannedStartDateMoreThanMaxAge(plannedStartDate, It.IsAny<DateTime?>())).Returns(true);

        var approvalDetails = CreatePlannedStartDateApprovalDetails(plannedStartDate);

        var result = await _service.DetermineCocUpdateStatusesAsync(approvalDetails.Updates, approvalDetails.Apprenticeship, approvalDetails.Course);

        result.Should().HaveCount(1);
        result[0].Status.Should().Be(CocApprovalItemStatus.AutoRejected);
        result[0].Reason.Should().Be($"The learner must be younger than {Constants.MaximumAgeAtApprenticeshipStart} years old at the start of their training");
    }

    [Test]
    public async Task DetermineCocUpdateStatusesAsync_ShouldAutoRejectPlannedStartDate_WhenLearnerExceedsMaximumAgeForLevel7()
    {
        var plannedStartDate = new DateTime(2026, 1, 1);

        _plannedStartDateValidationRulesMock.Setup(x => x.IsPlannedStartDateMoreThanMaxAgeForLevel7Course(plannedStartDate, It.IsAny<Apprenticeship>(), It.IsAny<Course>())).Returns(true);

        var approvalDetails = CreatePlannedStartDateApprovalDetails(plannedStartDate);

        var result = await _service.DetermineCocUpdateStatusesAsync(approvalDetails.Updates, approvalDetails.Apprenticeship, approvalDetails.Course);

        result.Should().HaveCount(1);
        result[0].Status.Should().Be(CocApprovalItemStatus.AutoRejected);
        result[0].Reason.Should().Be($"The learner must be younger than {Constants.MaximumAgeAtApprenticeshipStartForLevel7} years old at the start of their training");
    }

    [Test]
    public async Task DetermineCocUpdateStatusesAsync_ShouldAutoRejectPlannedStartDate_WhenEmailOverlapExists()
    {
        var plannedStartDate = new DateTime(2026, 1, 1);

        _plannedStartDateValidationRulesMock.Setup(x => x.IsThereEmailOverlapForPlannedStartDateAsync(plannedStartDate, It.IsAny<Apprenticeship>())).ReturnsAsync(true);

        var approvalDetails = CreatePlannedStartDateApprovalDetails(plannedStartDate);

        var result = await _service.DetermineCocUpdateStatusesAsync(approvalDetails.Updates, approvalDetails.Apprenticeship, approvalDetails.Course);

        result.Should().HaveCount(1);
        result[0].Status.Should().Be(CocApprovalItemStatus.AutoRejected);
        result[0].Reason.Should().Be("This email address is already used for another apprenticeship in the same training period - use a different email address");
    }

    private CocApprovalDetails CreatePlannedStartDateApprovalDetails(DateTime plannedStartDate)
    {
        return new CocApprovalDetails
        {
            Updates = new CocUpdates
            {
                PlannedStartDate = new CocUpdate<DateTime?>
                {
                    Old = plannedStartDate.AddDays(-1),
                    New = plannedStartDate
                }
            },
            Apprenticeship = new Apprenticeship
            {
                DateOfBirth = DateTime.UtcNow.AddYears(-20),
                Cohort = new Cohort()
            },
            Course = new Course
            {
                EffectiveFrom = plannedStartDate.AddMonths(-1),
                EffectiveTo = plannedStartDate.AddMonths(1)
            }
        };
    }
}