using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Entities.Reservations;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.TestHelpers.DatabaseMock;
using SFA.DAS.CommitmentsV2.Validation.CocApprovals;

namespace SFA.DAS.CommitmentsV2.UnitTests.Validation.CocApprovals;

[TestFixture]
public class PlannedStartDateValidationRulesTests
{
    private Mock<IAcademicYearDateProvider> _academicYearDateProvider;
    private Mock<IAgeCalculationService> _ageCalculationService;
    private Mock<IProviderCommitmentsDbContext> _dbContext;
    private Mock<IOverlapCheckService> _overlapCheckService;
    private Mock<IReservationValidationService> _reservationValidationService;

    private PlannedStartDateValidationRules _plannedStartDateRules;

    [SetUp]
    public void Arrange()
    {
        _academicYearDateProvider = new Mock<IAcademicYearDateProvider>();
        _ageCalculationService = new Mock<IAgeCalculationService>();
        _dbContext = new Mock<IProviderCommitmentsDbContext>();
        _overlapCheckService = new Mock<IOverlapCheckService>();
        _reservationValidationService = new Mock<IReservationValidationService>();

        _plannedStartDateRules = new PlannedStartDateValidationRules(
            _academicYearDateProvider.Object,
            _ageCalculationService.Object,
            _dbContext.Object,
            _overlapCheckService.Object,
            _reservationValidationService.Object);
    }

    [Test]
    public void IsPlannedStartDateBeforeAbsoluteMinimum_WhenBeforeMinimum_ReturnsTrue()
    {
        var result = _plannedStartDateRules.IsPlannedStartDateBeforeAbsoluteMinimum(Constants.DasStartDate.AddDays(-1));

        result.Should().BeTrue();
    }

    [Test]
    public void IsPlannedStartDateBeforeAbsoluteMinimum_WhenOnMinimum_ReturnsFalse()
    {
        var result = _plannedStartDateRules.IsPlannedStartDateBeforeAbsoluteMinimum(Constants.DasStartDate);

        result.Should().BeFalse();
    }

    [Test]
    public void IsPlannedStartDateAfterMaximum_WhenAfterAcademicYearEndPlusOneYear_ReturnsTrue()
    {
        _academicYearDateProvider.Setup(x => x.CurrentAcademicYearEndDate).Returns(new DateTime(2025, 7, 31));

        var result = _plannedStartDateRules.IsPlannedStartDateAfterMaximum(new DateTime(2026, 8, 1));

        result.Should().BeTrue();
    }

    [Test]
    public void IsPlannedStartDateBeforeFundingWindowHasClosed_WhenPlannedStartDateIsAfterAcademicYearStart_ReturnsFalse()
    {
        var academicYearStartDate = new DateTime(2025, 8, 1);

        _academicYearDateProvider.Setup(x => x.CurrentAcademicYearStartDate).Returns(academicYearStartDate);

        var plannedStartDate = academicYearStartDate.AddDays(1);

        var result = _plannedStartDateRules.IsPlannedStartDateBeforeFundingWindowHasClosed(plannedStartDate);
        result.Should().BeFalse();
    }

    [Test]
    public void IsPlannedStartDateBeforeLarsEffectiveFrom_WhenBeforeEffectiveFrom_ReturnsTrue()
    {
        var course = new Course
        {
            EffectiveFrom = new DateTime(2025, 8, 1)
        };

        var result = _plannedStartDateRules.IsPlannedStartDateBeforeLarsEffectiveFrom(new DateTime(2025, 7, 1), course);

        result.Should().BeTrue();
    }

    [Test]
    public void IsPlannedStartDateBeforeLarsEffectiveFrom_WhenEffectiveFromNull_ThrowsArgumentException()
    {
        var course = new Course
        {
            EffectiveFrom = null
        };

        Assert.Throws<ArgumentNullException>(() =>
            _plannedStartDateRules.IsPlannedStartDateBeforeLarsEffectiveFrom(DateTime.Today, course));
    }

    [Test]
    public void IsPlannedStartDateAfterLarsEffectiveTo_WhenAfterEffectiveTo_ReturnsTrue()
    {
        var course = new Course
        {
            EffectiveTo = new DateTime(2025, 7, 31)
        };

        var result = _plannedStartDateRules.IsPlannedStartDateAfterLarsEffectiveTo(new DateTime(2025, 8, 1), course);

        result.Should().BeTrue();
    }

    [Test]
    public void IsPlannedStartDateBeforeTransferFundedDate_WhenTransferFundedAndBeforeFeatureDate_ReturnsTrue()
    {
        var cohort = new Cohort
        {
            TransferSenderId = 123
        };

        var result = _plannedStartDateRules.IsPlannedStartDateBeforeTransferFundedDate(Constants.TransferFeatureStartDate.AddDays(-1), cohort);

        result.Should().BeTrue();
    }

    [Test]
    public void IsPlannedStartDateBeforeTransferFundedDate_WhenCohortNull_ReturnsFalse()
    {
        var result = _plannedStartDateRules.IsPlannedStartDateBeforeTransferFundedDate(DateTime.Today, null);

        result.Should().BeFalse();
    }

    [Test]
    public async Task IsPlannedStartDateOverlappingWithUlnDateRangeAsync_WhenOverlapExists_ReturnsTrue()
    {
        var plannedStartDate = new DateTime(2025, 2, 1);
        var apprenticeship = new Apprenticeship
        {
            Uln = "1234567890",
            Id = 1,
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 12, 31)
        };

        _overlapCheckService.Setup(x => x.CheckForOverlaps(apprenticeship.Uln, It.IsAny<CourseDateRange>(), apprenticeship.Id, CancellationToken.None))
            .ReturnsAsync(new OverlapCheckResult(true, true));

        var result = await _plannedStartDateRules.IsPlannedStartDateOverlappingWithUlnDateRangeAsync(plannedStartDate, apprenticeship);

        result.Should().BeTrue();
    }

    [Test]
    public async Task IsPlannedStartDateOverlappingWithUlnDateRangeAsync_WhenThereIsNoOverlap_ReturnsFalse()
    {
        var plannedStartDate = DateTime.Today;
        var apprenticeship = new Apprenticeship
        {
            Uln = "1234567890",
            Id = 1,
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 12, 31)
        };

        _overlapCheckService.Setup(x => x.CheckForOverlaps(apprenticeship.Uln, It.IsAny<CourseDateRange>(), apprenticeship.Id, CancellationToken.None))
            .ReturnsAsync(new OverlapCheckResult(false, false));

        var result = await _plannedStartDateRules.IsPlannedStartDateOverlappingWithUlnDateRangeAsync(plannedStartDate, apprenticeship);

        result.Should().BeFalse();
    }

    [Test]
    public async Task IsPlannedStartDateOverlappingWithUlnDateRangeAsync_WhenDatesMissing_ReturnsFalse()
    {
        var apprenticeship = new Apprenticeship
        {
            StartDate = null,
            EndDate = null
        };

        var result = await _plannedStartDateRules.IsPlannedStartDateOverlappingWithUlnDateRangeAsync(DateTime.Today, apprenticeship);

        result.Should().BeFalse();
    }

    [Test]
    public async Task IsPlannedStartDateValidForReservationAsync_WhenReservationMissing_ReturnsNull()
    {
        var apprenticeship = new Apprenticeship
        {
            ReservationId = null,
            CourseCode = "25"
        };

        var result = await _plannedStartDateRules.IsPlannedStartDateValidForReservationAsync(DateTime.Today, apprenticeship);

        result.Should().BeNull();
    }

    [Test]
    public async Task IsPlannedStartDateValidForReservationAsync_WhenReservationExists_ReturnsValidationResult()
    {
        var apprenticeship = new Apprenticeship
        {
            ReservationId = Guid.NewGuid(),
            CourseCode = "ABC123"
        };

        var expected = new ReservationValidationResult(null);

        _reservationValidationService.Setup(x => x.Validate(It.IsAny<ReservationValidationRequest>(), CancellationToken.None)).ReturnsAsync(expected);

        var result = await _plannedStartDateRules.IsPlannedStartDateValidForReservationAsync(DateTime.Today, apprenticeship);

        result.Should().BeSameAs(expected);
    }

    [Test]
    public void IsPlannedStartDateLessThanMinAge_WhenLearnerBelowMinimumAge_ReturnsTrue()
    {
        _ageCalculationService.Setup(x => x.WillLearnerBeAtLeastMinAgeAtStartOfTraining(It.IsAny<DateTime>(), It.IsAny<DateTime?>(), Constants.MinimumAgeAtApprenticeshipStart))
            .Returns(false);

        var result = _plannedStartDateRules.IsPlannedStartDateLessThanMinAge(DateTime.Today, DateTime.Today);

        result.Should().BeTrue();
    }

    [Test]
    public void IsPlannedStartDateMoreThanMaxAge_WhenLearnerTooOld_ReturnsTrue()
    {
        _ageCalculationService.Setup(x => x.LearnerAgeMustBeLessThenMaxAgeAtStartOfTraining(It.IsAny<DateTime>(), It.IsAny<DateTime?>(), Constants.MaximumAgeAtApprenticeshipStart))
            .Returns(false);

        var result = _plannedStartDateRules.IsPlannedStartDateMoreThanMaxAge(DateTime.Today, DateTime.Today.AddYears(-100));

        result.Should().BeTrue();
    }

    [Test]
    public void IsPlannedStartDateMoreThanMaxAgeForLevel7Course_WhenTooOld_ReturnsTrue()
    {
        _ageCalculationService.Setup(x => x.LearnerAgeMustBeLessThenMaxAgeAtStartOfTraining(It.IsAny<DateTime>(), It.IsAny<DateTime?>(), Constants.MaximumAgeAtApprenticeshipStartForLevel7))
            .Returns(false);

        var apprenticeship = new Apprenticeship
        {
            DateOfBirth = new DateTime(1960, 1, 1),
            Continuation = null
        };

        var course = new Course
        {
            LarsCode = "LarsCode",
            Level = "7"
        };

        var result = _plannedStartDateRules.IsPlannedStartDateMoreThanMaxAgeForLevel7Course(Constants.MaxAgeAt25RequiredOn, apprenticeship, course);

        result.Should().BeTrue();
    }

    [Test]
    public async Task IsThereEmailOverlapForPlannedStartDateAsync_WhenOverlapExists_ReturnsTrue()
    {
        var plannedStartDate = new DateTime(2026, 1, 2);
        var apprenticeship = new Apprenticeship
        {
            Id = 1,
            Email = "test@test.com",
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 12, 31),
            Cohort = new Cohort
            {
                Id = 99
            }
        };

        _overlapCheckService.Setup(x => x.CheckForEmailOverlaps(apprenticeship.Email, It.IsAny<CourseDateRange>(), apprenticeship.Id, 99, CancellationToken.None))
            .ReturnsAsync(new EmailOverlapCheckResult(2, OverlapStatus.OverlappingStartDate, true));

        var result = await _plannedStartDateRules.IsThereEmailOverlapForPlannedStartDateAsync(plannedStartDate, apprenticeship);

        result.Should().BeTrue();
    }

    [Test]
    public async Task IsThereEmailOverlapForPlannedStartDateAsync_WhenStartDateEqualsPlannedStartDate_ReturnsFalse()
    {
        var apprenticeship = new Apprenticeship
        {
            Id = 1,
            Email = "test@test.com",
            StartDate = DateTime.Today,
            EndDate = new DateTime(2025, 12, 31),
            Cohort = new Cohort
            {
                Id = 99
            }
        };

        var result = await _plannedStartDateRules.IsThereEmailOverlapForPlannedStartDateAsync(DateTime.Today, apprenticeship);

        result.Should().BeFalse();
    }

    [Test]
    public async Task IsThereEmailOverlapForPlannedStartDateAsync_WhenStartDateIsNull_ReturnsFalse()
    {
        var apprenticeship = new Apprenticeship
        {
            Id = 1,
            Email = "test@test.com",
            StartDate = null,
            EndDate = new DateTime(2025,12,31),
            Cohort = new Cohort
            {
                Id = 99
            }
        };

        var result = await _plannedStartDateRules.IsThereEmailOverlapForPlannedStartDateAsync(DateTime.Today, apprenticeship);

        result.Should().BeFalse();
    }

    [Test]
    public async Task IsThereEmailOverlapForPlannedStartDateAsync_WhenEndDateIsNull_ReturnsFalse()
    {
        var apprenticeship = new Apprenticeship
        {
            Id = 1,
            Email = "test@test.com",
            StartDate = new DateTime(2025, 1, 1),
            EndDate = null,
            Cohort = new Cohort
            {
                Id = 99
            }
        };

        var result = await _plannedStartDateRules.IsThereEmailOverlapForPlannedStartDateAsync(DateTime.Today, apprenticeship);

        result.Should().BeFalse();
    }

    [Test]
    public async Task IsThereEmailOverlapForPlannedStartDateAsync_WhenNoEmail_ReturnsFalse()
    {
        var apprenticeship = new Apprenticeship
        {
            Email = null
        };

        var result = await _plannedStartDateRules.IsThereEmailOverlapForPlannedStartDateAsync(DateTime.Today, apprenticeship);

        result.Should().BeFalse();
    }
}


