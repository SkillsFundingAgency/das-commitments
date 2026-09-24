using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Domain;
using SFA.DAS.CommitmentsV2.Domain.Entities.Reservations;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Shared.Extensions;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Validation.CocApprovals.Interfaces;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services;

[TestFixture]
public class CocApprovalStatusServiceTests
{
    private Mock<ILogger<CocApprovalStatusService>> _loggerMock;

    private Mock<IAcademicYearDateProvider> _academicYearDateProviderMock;
    private Mock<IPlannedStartDateValidationRules> _plannedStartDateValidationRulesMock;
    private CocApprovalStatusService _service;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<CocApprovalStatusService>>();
        _academicYearDateProviderMock = new Mock<IAcademicYearDateProvider>();
        _plannedStartDateValidationRulesMock = new Mock<IPlannedStartDateValidationRules>();
        _service = new CocApprovalStatusService(
            _plannedStartDateValidationRulesMock.Object,
            _academicYearDateProviderMock.Object,
            _loggerMock.Object);
    }

    [Test]
    public async Task DetermineCocUpdateStatusesAsync_ShouldThrow_WhenUpdatesIsNull()
    {
        var approvalDetails = new CocApprovalDetails
        {
            Updates = null,
            Apprenticeship = new Apprenticeship()
        };

        Func<Task> act = async () => await _service.DetermineCocUpdateStatusesAsync(approvalDetails);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("Updates");
    }

    [Test]
    public async Task DetermineCocUpdateStatusesAsync_ShouldThrow_WhenApprenticeshipIsNull()
    {
        var approvalDetails = new CocApprovalDetails
        {
            Updates = new CocUpdates(),
            Apprenticeship = null
        };

        Func<Task> act = async () => await _service.DetermineCocUpdateStatusesAsync(approvalDetails);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("Apprenticeship");
    }

    [Test]
    public async Task DetermineCocUpdateStatusesAsync_ShouldReturnEmpty_WhenNoTnpFieldsPresent()
    {
        var approvalDetails = new CocApprovalDetails
        {
            Updates = new CocUpdates(),
            Apprenticeship = new Apprenticeship { Cost = 1000 }
        };

        var result = await _service.DetermineCocUpdateStatusesAsync(approvalDetails);

        result.Should().BeEmpty();
    }

    [Test]
    public async Task DetermineCocUpdateStatusesAsync_ShouldLogInformation_WhenTnpFieldsPresent()
    {
        var approvalDetails = new CocApprovalDetails
        {
            Updates = new CocUpdates
            {
                TNP1 = new CocUpdate<int> { Old = 100, New = 90 }
            },
            Apprenticeship = new Apprenticeship
            {
                Cost = 1000
            }
        };

        await _service.DetermineCocUpdateStatusesAsync(approvalDetails);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, _) => o.ToString().Contains("Change of TNP1 or TNP2 detected")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Test]
    public async Task DetermineCocUpdateStatusesAsync_ShouldReturnPending_WhenOverallCourseCostRemainsTheSame()
    {
        var approvalDetails = new CocApprovalDetails
        {
            Updates = new CocUpdates
            {
                TNP1 = new CocUpdate<int> { Old = 100, New = 80 },
                TNP2 = new CocUpdate<int> { Old = 200, New = 220 }
            },
            Apprenticeship = new Apprenticeship
            {
                Cost = 300
            }
        };

        var result = await _service.DetermineCocUpdateStatusesAsync(approvalDetails);

        result.Should().HaveCount(2);
        result.Should().OnlyContain(r => r.Status == CocApprovalItemStatus.Pending);
    }

    [Test]
    public async Task DetermineCocUpdateStatusesAsync_ShouldReturnPending_WhenCostIncreases()
    {
        var approvalDetails = new CocApprovalDetails
        {
            Updates = new CocUpdates
            {
                TNP1 = new CocUpdate<int> { Old = 100, New = 200 },
                TNP2 = new CocUpdate<int> { Old = 102, New = 202 }
            },
            Apprenticeship = new Apprenticeship
            {
                Cost = 202
            }
        };

        var result = await _service.DetermineCocUpdateStatusesAsync(approvalDetails);

        result.Should().HaveCount(2);
        result[0].Status.Should().Be(CocApprovalItemStatus.Pending);
        result[0].Field.Should().Be(CocChangeField.TNP1);
        result[1].Status.Should().Be(CocApprovalItemStatus.Pending);
        result[1].Field.Should().Be(CocChangeField.TNP2);
    }

    [Test]
    public async Task DetermineCocUpdateStatusesAsync_ShouldReturnPending_WhenCostDecreases()
    {
        var approvalDetails = new CocApprovalDetails
        {
            Updates = new CocUpdates
            {
                TNP1 = new CocUpdate<int> { Old = 100, New = 95 },
                TNP2 = new CocUpdate<int> { Old = 102, New = 100 }
            },
            Apprenticeship = new Apprenticeship
            {
                Cost = 202
            }
        };

        var result = await _service.DetermineCocUpdateStatusesAsync(approvalDetails);

        result.Should().HaveCount(2);
        result[0].Status.Should().Be(CocApprovalItemStatus.Pending);
        result[0].Field.Should().Be(CocChangeField.TNP1);
        result[1].Status.Should().Be(CocApprovalItemStatus.Pending);
        result[1].Field.Should().Be(CocChangeField.TNP2);
    }

    [Test]
    public async Task DetermineCocUpdateStatusesAsync_ShouldLogWarning_WhenOldCostDoesNotMatchApprenticeshipCost()
    {
        var approvalDetails = new CocApprovalDetails
        {
            Updates = new CocUpdates
            {
                TNP1 = new CocUpdate<int> { Old = 10, New = 5 },
                TNP2 = new CocUpdate<int> { Old = 20, New = 15 }
            },
            Apprenticeship = new Apprenticeship
            {
                Cost = 500
            }
        };

        await _service.DetermineCocUpdateStatusesAsync(approvalDetails);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, _) =>
                    o.ToString().Contains("Old total cost from changes does not match apprenticeship cost")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Test]
    public async Task DetermineCocUpdateStatusesAsync_ShouldReturnAutoRejected_WhenTotalCost_ExceedsMaximumTotalTrainingCost()
    {
        var approvalDetails = new CocApprovalDetails
        {
            Updates = new CocUpdates
            {
                TNP1 = new CocUpdate<int> { Old = 100, New = 20000 },
                TNP2 = new CocUpdate<int> { Old = 102, New = 81000 }
            },
            Apprenticeship = new Apprenticeship
            {
                Cost = 202
            }
        };

        var result = await _service.DetermineCocUpdateStatusesAsync(approvalDetails);

        result.Should().HaveCount(2);
        result.Where(r => r.Field == CocChangeField.TNP1).First().Status.Should().Be(CocApprovalItemStatus.AutoRejected);
        result.Where(r => r.Field == CocChangeField.TNP2).First().Status.Should().Be(CocApprovalItemStatus.AutoRejected);
    }

    [Test]
    public async Task DetermineCocUpdateStatusesAsync_ShouldReturnAutoRejected_WhenTNP1_IsZero()
    {
        var approvalDetails = new CocApprovalDetails
        {
            Updates = new CocUpdates
            {
                TNP1 = new CocUpdate<int> { Old = 100, New = 0 },
                TNP2 = new CocUpdate<int> { Old = 102, New = 81000 }
            },
            Apprenticeship = new Apprenticeship
            {
                Cost = 202
            }
        };

        var result = await _service.DetermineCocUpdateStatusesAsync(approvalDetails);

        result.Should().HaveCount(2);
        result[0].Field.Should().Be(CocChangeField.TNP1);
        result[0].Status.Should().Be(CocApprovalItemStatus.AutoRejected);
        result[1].Field.Should().Be(CocChangeField.TNP2);
        result[1].Status.Should().Be(CocApprovalItemStatus.AutoRejected);
    }

    [Test]
    public async Task DetermineCocUpdateStatusesAsync_ShouldAutoApprovePlannedStartDate_WhenAllValidationRulesPass()
    {
        var plannedStartDate = new DateTime(2027, 1, 1);

        _plannedStartDateValidationRulesMock.Setup(x => x.IsPlannedStartDateValidForReservationAsync(plannedStartDate,It.IsAny<Apprenticeship>())).ReturnsAsync((ReservationValidationResult)null);
        
        var approvalDetails = CreatePlannedStartDateApprovalDetails(plannedStartDate);
        
        var result = await _service.DetermineCocUpdateStatusesAsync(approvalDetails);
        
        result.Should().ContainSingle(x => x.Field == CocChangeField.PlannedStartDate && x.Status == CocApprovalItemStatus.AutoApproved);
    }

    [Test]
    public async Task DetermineCocUpdateStatusesAsync_ShouldAutoReject_WhenPlannedStartDateBeforeAbsoluteMinimum()
    {
        var plannedStartDate = new DateTime(2016, 1, 1);

        _plannedStartDateValidationRulesMock.Setup(x => x.IsPlannedStartDateBeforeAbsoluteMinimum(plannedStartDate)).Returns(true);

        var approvalDetails = CreatePlannedStartDateApprovalDetails(plannedStartDate);

        var result = await _service.DetermineCocUpdateStatusesAsync(approvalDetails);

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

        var result = await _service.DetermineCocUpdateStatusesAsync(approvalDetails);
        
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

        var result = await _service.DetermineCocUpdateStatusesAsync(approvalDetails);

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

        var result = await _service.DetermineCocUpdateStatusesAsync(approvalDetails);

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

        var result = await _service.DetermineCocUpdateStatusesAsync(approvalDetails);

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

        var result = await _service.DetermineCocUpdateStatusesAsync(approvalDetails);

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

        var result = await _service.DetermineCocUpdateStatusesAsync(approvalDetails);

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

        var result = await _service.DetermineCocUpdateStatusesAsync(approvalDetails);

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

        var result = await _service.DetermineCocUpdateStatusesAsync(approvalDetails);

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

        var result = await _service.DetermineCocUpdateStatusesAsync(approvalDetails);

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

        var result = await _service.DetermineCocUpdateStatusesAsync(approvalDetails);

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

        var result = await _service.DetermineCocUpdateStatusesAsync(approvalDetails);

        result.Should().HaveCount(1);
        result[0].Status.Should().Be(CocApprovalItemStatus.AutoRejected);
        result[0].Reason.Should().Be("This email address is already used for another apprenticeship in the same training period - use a different email address");
    }

    private CocApprovalDetails CreatePlannedStartDateApprovalDetails(DateTime plannedStartDate)
    {
        return new CocApprovalDetails
        {
            Updates = new CocUpdates(),
            Apprenticeship = new Apprenticeship
            {
                DateOfBirth = DateTime.UtcNow.AddYears(-20),
                Cohort = new Cohort()
            },
            Course = new Course
            {
                EffectiveFrom = plannedStartDate.AddMonths(-1),
                EffectiveTo = plannedStartDate.AddMonths(1)
            },
            ApprovalFieldChanges = new List<CocApprovalFieldChange>
            {
                new CocApprovalFieldChange
                {
                    ChangeType = nameof(CocChangeField.PlannedStartDate),
                    Data = new CocData
                    {
                        Old = plannedStartDate.AddDays(-1).ToShortDateString(),
                        New = plannedStartDate.ToShortDateString()
                    }
                }
            }
        };
    }

}