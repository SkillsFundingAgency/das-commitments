using SFA.DAS.CommitmentsV2.Domain;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Entities.Reservations;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Validation.CocApprovals;

namespace SFA.DAS.CommitmentsV2.UnitTests.Validation.CocApprovals;

[TestFixture]
public class CocApprovalStartDateValidationRulesTests
{
    private Mock<IAcademicYearDateProvider> _academicYearDateProvider;
    private Mock<ICurrentDateTime> _currentDateTime;
    private Mock<IAgeCalculationService> _ageCalculationService;
    private Mock<IOverlapCheckService> _overlapCheckService;
    private Mock<IReservationValidationService> _reservationValidationService;

    private CocApprovalStartDateValidationRules _cocApprovalStartDateValidationRules;

    [SetUp]
    public void Arrange()
    {
        _academicYearDateProvider = new Mock<IAcademicYearDateProvider>();
        _currentDateTime = new Mock<ICurrentDateTime>();
        _ageCalculationService = new Mock<IAgeCalculationService>();
        _overlapCheckService = new Mock<IOverlapCheckService>();
        _reservationValidationService = new Mock<IReservationValidationService>();

        _cocApprovalStartDateValidationRules = new CocApprovalStartDateValidationRules(
            _academicYearDateProvider.Object,
            _currentDateTime.Object,
            _ageCalculationService.Object,
            _overlapCheckService.Object,
            _reservationValidationService.Object);
    }

    [Test]
    public void IsStartDateBeforeAbsoluteMinimum_WhenBeforeMinimum_ReturnsTrue()
    {
        var result = _cocApprovalStartDateValidationRules.IsStartDateBeforeAbsoluteMinimum(Constants.DasStartDate.AddDays(-1));

        result.Should().BeTrue();
    }

    [Test]
    public void IsStartDateBeforeAbsoluteMinimum_WhenOnMinimum_ReturnsFalse()
    {
        var result = _cocApprovalStartDateValidationRules.IsStartDateBeforeAbsoluteMinimum(Constants.DasStartDate);

        result.Should().BeFalse();
    }

    [Test]
    public void IsStartDateAfterMaximum_WhenAfterAcademicYearEndPlusOneYear_ReturnsTrue()
    {
        _academicYearDateProvider.Setup(x => x.CurrentAcademicYearEndDate).Returns(new DateTime(2025, 7, 31));

        var result = _cocApprovalStartDateValidationRules.IsStartDateAfterMaximum(new DateTime(2026, 8, 1));

        result.Should().BeTrue();
    }

    [Test]
    public void IsStartDateBeforeFundingWindowHasClosed_WhenFundingWindowHasClosedAndStartDateIsBeforeAcademicYearStart_ReturnsTrue()
    {
        var academicYearStartDate = new DateTime(2026, 8, 1);
        var lastAcademicYearFundingPeriod = new DateTime(2026, 10, 18);

        _academicYearDateProvider.Setup(x => x.CurrentAcademicYearStartDate).Returns(academicYearStartDate);
        _academicYearDateProvider.Setup(x => x.LastAcademicYearFundingPeriod).Returns(lastAcademicYearFundingPeriod);
        _currentDateTime.Setup(x => x.UtcNow).Returns(lastAcademicYearFundingPeriod.AddDays(1));

        var result = _cocApprovalStartDateValidationRules.IsStartDateBeforeFundingWindowHasClosed(academicYearStartDate.AddDays(-1));

        result.Should().BeTrue();
    }

    [Test]
    public void IsStartDateBeforeFundingWindowHasClosed_WhenFundingWindowHasNotClosed_ReturnsFalse()
    {
        var academicYearStartDate = new DateTime(2026, 8, 1);
        var lastAcademicYearFundingPeriod = new DateTime(2026, 10, 18);

        _academicYearDateProvider.Setup(x => x.CurrentAcademicYearStartDate).Returns(academicYearStartDate);
        _academicYearDateProvider.Setup(x => x.LastAcademicYearFundingPeriod).Returns(lastAcademicYearFundingPeriod);
        _currentDateTime.Setup(x => x.UtcNow).Returns(lastAcademicYearFundingPeriod);

        var result = _cocApprovalStartDateValidationRules.IsStartDateBeforeFundingWindowHasClosed(academicYearStartDate.AddDays(-1));

        result.Should().BeFalse();
    }

    [Test]
    public void IsStartDateBeforeFundingWindowHasClosed_WhenStartDateIsOnAcademicYearStart_ReturnsFalse()
    {
        var academicYearStartDate = new DateTime(2026, 8, 1);
        var lastAcademicYearFundingPeriod = new DateTime(2026, 10, 18);

        _academicYearDateProvider.Setup(x => x.CurrentAcademicYearStartDate).Returns(academicYearStartDate);
        _academicYearDateProvider.Setup(x => x.LastAcademicYearFundingPeriod).Returns(lastAcademicYearFundingPeriod);
        _currentDateTime.Setup(x => x.UtcNow).Returns(lastAcademicYearFundingPeriod.AddDays(1));

        var result = _cocApprovalStartDateValidationRules.IsStartDateBeforeFundingWindowHasClosed(academicYearStartDate);

        result.Should().BeFalse();
    }

    [Test]
    public void IsStartDateBeforeFundingWindowHasClosed_WhenStartDateIsAfterAcademicYearStart_ReturnsFalse()
    {
        var academicYearStartDate = new DateTime(2026, 8, 1);
        var lastAcademicYearFundingPeriod = new DateTime(2026, 10, 18);

        _academicYearDateProvider.Setup(x => x.CurrentAcademicYearStartDate).Returns(academicYearStartDate);
        _academicYearDateProvider.Setup(x => x.LastAcademicYearFundingPeriod).Returns(lastAcademicYearFundingPeriod);
        _currentDateTime.Setup(x => x.UtcNow).Returns(lastAcademicYearFundingPeriod.AddDays(1));

        var result = _cocApprovalStartDateValidationRules.IsStartDateBeforeFundingWindowHasClosed(academicYearStartDate.AddDays(1));

        result.Should().BeFalse();
    }

    [Test]
    public void IsActualStartDateBeforeLarsEffectiveFrom_WhenBeforeEffectiveFrom_ReturnsTrue()
    {
        var course = new Course
        {
            EffectiveFrom = new DateTime(2025, 8, 1)
        };

        var result = _cocApprovalStartDateValidationRules.IsActualStartDateBeforeLarsEffectiveFrom(new DateTime(2025, 7, 1), course);

        result.Should().BeTrue();
    }

    [Test]
    public void IsActualStartDateAfterLarsEffectiveTo_WhenAfterEffectiveTo_ReturnsTrue()
    {
        var course = new Course
        {
            EffectiveTo = new DateTime(2025, 7, 31)
        };

        var result = _cocApprovalStartDateValidationRules.IsActualStartDateAfterLarsEffectiveTo(new DateTime(2025, 8, 1), course);

        result.Should().BeTrue();
    }

    [Test]
    public void IsStartDateBeforeTransferFundedDate_WhenTransferFundedAndBeforeFeatureDate_ReturnsTrue()
    {
        var cohort = new Cohort
        {
            TransferSenderId = 123
        };

        var result = _cocApprovalStartDateValidationRules.IsStartDateBeforeTransferFundedDate(Constants.TransferFeatureStartDate.AddDays(-1), cohort);

        result.Should().BeTrue();
    }

    [Test]
    public void IsStartDateBeforeTransferFundedDate_WhenCohortNull_ReturnsFalse()
    {
        var result = _cocApprovalStartDateValidationRules.IsStartDateBeforeTransferFundedDate(DateTime.Today, null);

        result.Should().BeFalse();
    }

    [Test]
    public async Task IsStartDateOverlappingWithUlnDateRangeAsync_WhenOverlapExists_ReturnsTrue()
    {
        var startDate = new DateTime(2025, 2, 1);
        var apprenticeship = new Apprenticeship
        {
            Uln = "1234567890",
            Id = 1,
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 12, 31)
        };

        _overlapCheckService.Setup(x => x.CheckForOverlaps(apprenticeship.Uln, It.IsAny<CourseDateRange>(), apprenticeship.Id, CancellationToken.None))
            .ReturnsAsync(new OverlapCheckResult(true, true));

        var result = await _cocApprovalStartDateValidationRules.IsStartDateOverlappingWithUlnDateRangeAsync(startDate, apprenticeship);

        result.Should().BeTrue();
    }

    [Test]
    public async Task IsStartDateOverlappingWithUlnDateRangeAsync_WhenThereIsNoOverlap_ReturnsFalse()
    {
        var startDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var apprenticeship = new Apprenticeship
        {
            Uln = "1234567890",
            Id = 1,
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 12, 31)
        };

        _overlapCheckService.Setup(x => x.CheckForOverlaps(apprenticeship.Uln, It.IsAny<CourseDateRange>(), apprenticeship.Id, CancellationToken.None))
            .ReturnsAsync(new OverlapCheckResult(false, false));

        var result = await _cocApprovalStartDateValidationRules.IsStartDateOverlappingWithUlnDateRangeAsync(startDate, apprenticeship);

        result.Should().BeFalse();
    }

    [Test]
    public async Task IsStartDateOverlappingWithUlnDateRangeAsync_WhenDatesMissing_ReturnsFalse()
    {
        var startDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var apprenticeship = new Apprenticeship
        {
            StartDate = null,
            EndDate = null
        };

        var result = await _cocApprovalStartDateValidationRules.IsStartDateOverlappingWithUlnDateRangeAsync(startDate, apprenticeship);

        result.Should().BeFalse();
    }

    [Test]
    public async Task IsStartDateValidForReservationAsync_WhenReservationMissing_ReturnsNull()
    {
        var startDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var apprenticeship = new Apprenticeship
        {
            ReservationId = null,
            CourseCode = "25"
        };

        var result = await _cocApprovalStartDateValidationRules.IsStartDateValidForReservationAsync(startDate, apprenticeship);

        result.Should().BeNull();
    }

    [Test]
    public async Task IsStartDateValidForReservationAsync_WhenReservationExists_ReturnsValidationResult()
    {
        var startDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var apprenticeship = new Apprenticeship
        {
            ReservationId = Guid.NewGuid(),
            CourseCode = "ABC123"
        };

        var expected = new ReservationValidationResult(null);

        _reservationValidationService.Setup(x => x.Validate(It.IsAny<ReservationValidationRequest>(), CancellationToken.None)).ReturnsAsync(expected);

        var result = await _cocApprovalStartDateValidationRules.IsStartDateValidForReservationAsync(startDate, apprenticeship);

        result.Should().BeSameAs(expected);
    }

    [Test]
    public void IsStartDateLessThanMinAge_WhenLearnerBelowMinimumAge_ReturnsTrue()
    {
        var startDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        _ageCalculationService.Setup(x => x.WillLearnerBeAtLeastMinAgeAtStartOfTraining(It.IsAny<DateTime>(), It.IsAny<DateTime?>(), Constants.MinimumAgeAtApprenticeshipStart))
            .Returns(false);

        var result = _cocApprovalStartDateValidationRules.IsStartDateLessThanMinAge(startDate, startDate);

        result.Should().BeTrue();
    }

    [Test]
    public void IsStartDateMoreThanMaxAge_WhenLearnerTooOld_ReturnsTrue()
    {
        var startDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        _ageCalculationService.Setup(x => x.LearnerAgeMustBeLessThenMaxAgeAtStartOfTraining(It.IsAny<DateTime>(), It.IsAny<DateTime?>(), Constants.MaximumAgeAtApprenticeshipStart))
            .Returns(false);

        var result = _cocApprovalStartDateValidationRules.IsStartDateMoreThanMaxAge(startDate, startDate.AddYears(-100));

        result.Should().BeTrue();
    }

    [Test]
    public void IsStartDateMoreThanMaxAgeForLevel7Course_WhenTooOld_ReturnsTrue()
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

        var result = _cocApprovalStartDateValidationRules.IsStartDateMoreThanMaxAgeForLevel7Course(Constants.MaxAgeAt25RequiredOn, apprenticeship, course);

        result.Should().BeTrue();
    }

    [Test]
    public async Task IsThereEmailOverlapForStartDateAsync_WhenOverlapExists_ReturnsTrue()
    {
        var startDate = new DateTime(2026, 1, 1);
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

        var result = await _cocApprovalStartDateValidationRules.IsThereEmailOverlapForStartDateAsync(startDate, apprenticeship);

        result.Should().BeTrue();
    }

    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(null, true)]
    public async Task IsThereEmailOverlapForStartDateAsync_WhenEmailAddressConfirmed(bool? emailAddressConfirmed, bool expectedResult)
    {
        var startDate = new DateTime(2026, 1, 1);
        var apprenticeship = new Apprenticeship
        {
            Id = 1,
            Email = "test@test.com",
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 12, 31),
            Cohort = new Cohort
            {
                Id = 99
            },
            EmailAddressConfirmed = emailAddressConfirmed
        };

        _overlapCheckService.Setup(x => x.CheckForEmailOverlaps(apprenticeship.Email, It.IsAny<CourseDateRange>(), apprenticeship.Id, 99, CancellationToken.None))
            .ReturnsAsync(new EmailOverlapCheckResult(2, OverlapStatus.OverlappingStartDate, true));

        var result = await _cocApprovalStartDateValidationRules.IsThereEmailOverlapForStartDateAsync(startDate, apprenticeship);

        result.Should().Be(expectedResult);
    }

    [Test]
    public async Task IsThereEmailOverlapForStartDateAsync_WhenApprenticeshipStartDateEqualsStartDate_ReturnsFalse()
    {
        var startDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var apprenticeship = new Apprenticeship
        {
            Id = 1,
            Email = "test@test.com",
            StartDate = startDate,
            EndDate = new DateTime(2025, 12, 31),
            Cohort = new Cohort
            {
                Id = 99
            }
        };

        var result = await _cocApprovalStartDateValidationRules.IsThereEmailOverlapForStartDateAsync(startDate, apprenticeship);

        result.Should().BeFalse();
    }

    [Test]
    public async Task IsThereEmailOverlapForStartDateAsync_WhenStartDateIsNull_ReturnsFalse()
    {
        var startDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
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

        var result = await _cocApprovalStartDateValidationRules.IsThereEmailOverlapForStartDateAsync(startDate, apprenticeship);

        result.Should().BeFalse();
    }

    [Test]
    public async Task IsThereEmailOverlapForStartDateAsync_WhenEndDateIsNull_ReturnsFalse()
    {
        var startDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
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

        var result = await _cocApprovalStartDateValidationRules.IsThereEmailOverlapForStartDateAsync(startDate, apprenticeship);

        result.Should().BeFalse();
    }

    [Test]
    public async Task IsThereEmailOverlapForStartDateAsync_WhenNoEmail_ReturnsFalse()
    {
        var startDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var apprenticeship = new Apprenticeship
        {
            Email = null
        };

        var result = await _cocApprovalStartDateValidationRules.IsThereEmailOverlapForStartDateAsync(startDate, apprenticeship);

        result.Should().BeFalse();
    }
}