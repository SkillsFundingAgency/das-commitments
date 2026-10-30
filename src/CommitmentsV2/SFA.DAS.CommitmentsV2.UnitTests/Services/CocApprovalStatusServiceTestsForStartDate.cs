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

public class CocApprovalStatusServiceTestsForStartDate
{
    private Mock<ILogger<CocApprovalStatusService>> _loggerMock;
    private Mock<IOverlapCheckService> _overlapCheckServiceMock;
    private Mock<IAcademicYearDateProvider> _academicYearDateProviderMock;
    private Mock<ICocApprovalStartDateValidationRules> _cocApprovalStartDateValidationRulesMock;
    private CocApprovalStatusService _service;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<CocApprovalStatusService>>();
        _overlapCheckServiceMock = new Mock<IOverlapCheckService>();
        _academicYearDateProviderMock = new Mock<IAcademicYearDateProvider>();
        _cocApprovalStartDateValidationRulesMock = new Mock<ICocApprovalStartDateValidationRules>();
        _service = new CocApprovalStatusService(_overlapCheckServiceMock.Object, _cocApprovalStartDateValidationRulesMock.Object, _academicYearDateProviderMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task DetermineCocUpdateStatuses_ShouldAutoApprovePlannedStartDate_WhenAllValidationRulesPass()
    {
        var plannedStartDate = new DateTime(2027, 1, 1);

        _cocApprovalStartDateValidationRulesMock.Setup(x => x.IsStartDateValidForReservationAsync(plannedStartDate, It.IsAny<Apprenticeship>())).ReturnsAsync((ReservationValidationResult)null);

        var approvalDetails = CreatePlannedStartDateApprovalDetails(plannedStartDate);

        var result = await _service.DetermineCocUpdateStatuses(approvalDetails.Updates, approvalDetails.Apprenticeship, approvalDetails.Course);

        result.Should().ContainSingle(x => x.Field == CocChangeField.PlannedStartDate && x.Status == CocApprovalItemStatus.AutoApproved);
    }

    [Test]
    public async Task DetermineCocUpdateStatuses_ShouldAutoReject_WhenPlannedStartDateBeforeAbsoluteMinimum()
    {
        var plannedStartDate = new DateTime(2016, 1, 1);

        _cocApprovalStartDateValidationRulesMock.Setup(x => x.IsStartDateBeforeAbsoluteMinimum(plannedStartDate)).Returns(true);

        var approvalDetails = CreatePlannedStartDateApprovalDetails(plannedStartDate);

        var result = await _service.DetermineCocUpdateStatuses(approvalDetails.Updates, approvalDetails.Apprenticeship, approvalDetails.Course);

        result.Should().HaveCount(1);
        result[0].Field.Should().Be(CocChangeField.PlannedStartDate);
        result[0].Status.Should().Be(CocApprovalItemStatus.AutoRejected);
        result[0].Reason.Should().Be("The start date must not be earlier than May 2017");
    }

    [Test]
    public async Task DetermineCocUpdateStatuses_ShouldAutoReject_WhenPlannedStartDateAfterMaximum()
    {
        var plannedStartDate = new DateTime(2030, 1, 1);

        _cocApprovalStartDateValidationRulesMock.Setup(x => x.IsStartDateAfterMaximum(plannedStartDate)).Returns(true);

        var approvalDetails = CreatePlannedStartDateApprovalDetails(plannedStartDate);

        var result = await _service.DetermineCocUpdateStatuses(approvalDetails.Updates, approvalDetails.Apprenticeship, approvalDetails.Course);

        result.Should().HaveCount(1);
        result[0].Status.Should().Be(CocApprovalItemStatus.AutoRejected);
        result[0].Reason.Should().Be("The start date must be no later than one year after the end of the current teaching year");
    }

    [Test]
    public async Task DetermineCocUpdateStatuses_ShouldAutoReject_WhenPlannedStartDateBeforeFundingWindowHasClosed()
    {
        var plannedStartDate = new DateTime(2024, 1, 1);
        var academicYearStart = new DateTime(2025, 8, 1);

        _cocApprovalStartDateValidationRulesMock.Setup(x => x.IsStartDateBeforeFundingWindowHasClosed(plannedStartDate)).Returns(true);

        _academicYearDateProviderMock.Setup(x => x.CurrentAcademicYearStartDate).Returns(academicYearStart);

        var approvalDetails = CreatePlannedStartDateApprovalDetails(plannedStartDate);

        var result = await _service.DetermineCocUpdateStatuses(approvalDetails.Updates, approvalDetails.Apprenticeship, approvalDetails.Course);

        result.Should().HaveCount(1);
        result[0].Status.Should().Be(CocApprovalItemStatus.AutoRejected);
        result[0].Reason.Should().Be($"The earliest start date you can use is {academicYearStart.ToGdsFormatShortMonthWithoutDay()}");
    }

    [Test]
    public async Task DetermineCocUpdateStatuses_ShouldAutoReject_WhenPlannedStartDateBeforeLarsEffectiveFrom()
    {
        var plannedStartDate = new DateTime(2025, 1, 1);
        var effectiveFrom = new DateTime(2025, 5, 1);

        _cocApprovalStartDateValidationRulesMock.Setup(x => x.IsActualStartDateBeforeLarsEffectiveFrom(plannedStartDate, It.IsAny<Course>())).Returns(true);

        var approvalDetails = CreatePlannedStartDateApprovalDetails(plannedStartDate);
        approvalDetails.Course.EffectiveFrom = effectiveFrom;

        var result = await _service.DetermineCocUpdateStatuses(approvalDetails.Updates, approvalDetails.Apprenticeship, approvalDetails.Course);

        result.Should().HaveCount(1);
        result[0].Status.Should().Be(CocApprovalItemStatus.AutoRejected);
        result[0].Reason.Should().Be("This training course is only available to learners with a start date after 04 2025");
    }

    [Test]
    public async Task DetermineCocUpdateStatuses_ShouldAutoReject_WhenPlannedStartDateAfterLarsEffectiveTo()
    {
        var plannedStartDate = new DateTime(2025, 1, 1);
        var effectiveTo = new DateTime(2025, 5, 1);

        _cocApprovalStartDateValidationRulesMock.Setup(x => x.IsActualStartDateAfterLarsEffectiveTo(plannedStartDate, It.IsAny<Course>())).Returns(true);

        var approvalDetails = CreatePlannedStartDateApprovalDetails(plannedStartDate);
        approvalDetails.Course.EffectiveTo = effectiveTo;

        var result = await _service.DetermineCocUpdateStatuses(approvalDetails.Updates, approvalDetails.Apprenticeship, approvalDetails.Course);

        result.Should().HaveCount(1);
        result[0].Status.Should().Be(CocApprovalItemStatus.AutoRejected);
        result[0].Reason.Should().Be("This training course is only available to learners with a start date before 06 2025");
    }

    [Test]
    public async Task DetermineCocUpdateStatuses_ShouldAutoReject_WhenPlannedStartDateBeforeTransferFundedDate()
    {
        var plannedStartDate = new DateTime(2017, 1, 1);

        _cocApprovalStartDateValidationRulesMock.Setup(x => x.IsStartDateBeforeTransferFundedDate(plannedStartDate, It.IsAny<Cohort>())).Returns(true);

        var approvalDetails = CreatePlannedStartDateApprovalDetails(plannedStartDate);

        var result = await _service.DetermineCocUpdateStatuses(approvalDetails.Updates, approvalDetails.Apprenticeship, approvalDetails.Course);

        result.Should().HaveCount(1);
        result[0].Status.Should().Be(CocApprovalItemStatus.AutoRejected);
        result[0].Reason.Should().Be("Learners funded through a transfer can't start earlier than May 2018");
    }

    [Test]
    public async Task DetermineCocUpdateStatusesAync_ShouldAutoReject_WhenPlannedStartDateUlnDateRangeOverlaps()
    {
        var plannedStartDate = new DateTime(2026, 1, 1);

        _cocApprovalStartDateValidationRulesMock.Setup(x => x.IsStartDateOverlappingWithUlnDateRangeAsync(plannedStartDate, It.IsAny<Apprenticeship>())).ReturnsAsync(true);

        var approvalDetails = CreatePlannedStartDateApprovalDetails(plannedStartDate);

        var result = await _service.DetermineCocUpdateStatuses(approvalDetails.Updates, approvalDetails.Apprenticeship, approvalDetails.Course);

        result.Should().HaveCount(1);
        result[0].Status.Should().Be(CocApprovalItemStatus.AutoRejected);
        result[0].Reason.Should().Be("Learners overlapping dates and therefore cant start");
    }

    [Test]
    public async Task DetermineCocUpdateStatuses_ShouldAutoReject_WhenPlannedStartDateReservationValidationFails()
    {
        var plannedStartDate = new DateTime(2026, 1, 1);

        var reservationValidationErrors = new ReservationValidationError[]
        {
            new ReservationValidationError("Property", "Reservation validation failed")
        };

        _cocApprovalStartDateValidationRulesMock.Setup(x => x.IsStartDateValidForReservationAsync(plannedStartDate, It.IsAny<Apprenticeship>())).ReturnsAsync(new ReservationValidationResult(reservationValidationErrors));

        var approvalDetails = CreatePlannedStartDateApprovalDetails(plannedStartDate);

        var result = await _service.DetermineCocUpdateStatuses(approvalDetails.Updates, approvalDetails.Apprenticeship, approvalDetails.Course);

        result.Should().HaveCount(1);
        result[0].Status.Should().Be(CocApprovalItemStatus.AutoRejected);
        result[0].Reason.Should().Be("Reservation validation failed");
    }

    [Test]
    public async Task DetermineCocUpdateStatuses_ShouldAutoRejectPlannedStartDate_WhenLearnerIsTooYoung()
    {
        var plannedStartDate = new DateTime(2026, 1, 1);

        _cocApprovalStartDateValidationRulesMock.Setup(x => x.IsStartDateLessThanMinAge(plannedStartDate, It.IsAny<DateTime?>())).Returns(true);

        var approvalDetails = CreatePlannedStartDateApprovalDetails(plannedStartDate);

        var result = await _service.DetermineCocUpdateStatuses(approvalDetails.Updates, approvalDetails.Apprenticeship, approvalDetails.Course);

        result.Should().HaveCount(1);
        result[0].Status.Should().Be(CocApprovalItemStatus.AutoRejected);
        result[0].Reason.Should().Be($"The learner must be at least {Constants.MinimumAgeAtApprenticeshipStart} years old at the start of their training");
    }

    [Test]
    public async Task DetermineCocUpdateStatuses_ShouldAutoRejectPlannedStartDate_WhenLearnerExceedsMaximumAge()
    {
        var plannedStartDate = new DateTime(2026, 1, 1);

        _cocApprovalStartDateValidationRulesMock.Setup(x => x.IsStartDateMoreThanMaxAge(plannedStartDate, It.IsAny<DateTime?>())).Returns(true);

        var approvalDetails = CreatePlannedStartDateApprovalDetails(plannedStartDate);

        var result = await _service.DetermineCocUpdateStatuses(approvalDetails.Updates, approvalDetails.Apprenticeship, approvalDetails.Course);

        result.Should().HaveCount(1);
        result[0].Status.Should().Be(CocApprovalItemStatus.AutoRejected);
        result[0].Reason.Should().Be($"The learner must be younger than {Constants.MaximumAgeAtApprenticeshipStart} years old at the start of their training");
    }

    [Test]
    public async Task DetermineCocUpdateStatuses_ShouldAutoRejectPlannedStartDate_WhenLearnerExceedsMaximumAgeForLevel7()
    {
        var plannedStartDate = new DateTime(2026, 1, 1);

        _cocApprovalStartDateValidationRulesMock.Setup(x => x.IsStartDateMoreThanMaxAgeForLevel7Course(plannedStartDate, It.IsAny<Apprenticeship>(), It.IsAny<Course>())).Returns(true);

        var approvalDetails = CreatePlannedStartDateApprovalDetails(plannedStartDate);

        var result = await _service.DetermineCocUpdateStatuses(approvalDetails.Updates, approvalDetails.Apprenticeship, approvalDetails.Course);

        result.Should().HaveCount(1);
        result[0].Status.Should().Be(CocApprovalItemStatus.AutoRejected);
        result[0].Reason.Should().Be($"The learner must be younger than {Constants.MaximumAgeAtApprenticeshipStartForLevel7} years old at the start of their training");
    }

    [Test]
    public async Task DetermineCocUpdateStatuses_ShouldAutoRejectPlannedStartDate_WhenEmailOverlapExists()
    {
        var plannedStartDate = new DateTime(2026, 1, 1);

        _cocApprovalStartDateValidationRulesMock.Setup(x => x.IsThereEmailOverlapForStartDateAsync(plannedStartDate, It.IsAny<Apprenticeship>())).ReturnsAsync(true);

        var approvalDetails = CreatePlannedStartDateApprovalDetails(plannedStartDate);

        var result = await _service.DetermineCocUpdateStatuses(approvalDetails.Updates, approvalDetails.Apprenticeship, approvalDetails.Course);

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