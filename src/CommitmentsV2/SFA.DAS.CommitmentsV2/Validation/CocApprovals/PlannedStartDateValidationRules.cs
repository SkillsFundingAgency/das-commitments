using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Entities.Reservations;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Validation.CocApprovals.Interfaces;

namespace SFA.DAS.CommitmentsV2.Validation.CocApprovals;

public class PlannedStartDateValidationRules(
    IAcademicYearDateProvider academicYearDateProvider,
    IAgeCalculationService ageCalculationService,
    IProviderCommitmentsDbContext dbContext,
    IOverlapCheckService overlapCheckService,
    IReservationValidationService reservationValidationService) 
    :IPlannedStartDateValidationRules
{
    public bool IsPlannedStartDateBeforeAbsoluteMinimum(DateTime plannedStartDate)
    {
        return plannedStartDate < Constants.DasStartDate;
    }
    public bool IsPlannedStartDateAfterMaximum(DateTime plannedStartDate)
    {
        return plannedStartDate > academicYearDateProvider.CurrentAcademicYearEndDate.AddYears(1);
    }

    public bool IsPlannedStartDateBeforeFundingWindowHasClosed(DateTime plannedStartDate)
    {
        return (DateTime.UtcNow > Constants.FundingWindowClosedOn) && (plannedStartDate < academicYearDateProvider.CurrentAcademicYearStartDate);
    }

    public bool IsPlannedStartDateBeforeLarsEffectiveFrom(DateTime plannedStartDate, Course course)
    {
        ArgumentNullException.ThrowIfNull(course.EffectiveFrom);
        return plannedStartDate < course.EffectiveFrom;
    }

    public bool IsPlannedStartDateAfterLarsEffectiveTo(DateTime plannedStartDate, Course course)
    {
        return plannedStartDate > course.EffectiveTo;
    }

    public bool IsPlannedStartDateBeforeTransferFundedDate(DateTime plannedStartDate, Cohort cohort)
    {
        if (cohort != null)
        {
            return cohort.TransferSenderId.HasValue && plannedStartDate < Constants.TransferFeatureStartDate;
        }

        return false;
    }

    public async Task<bool> IsPlannedStartDateOverlappingWithUlnDateRangeAsync(DateTime plannedStartDate, Apprenticeship apprenticeship)
    {
        if (apprenticeship.EndDate != null)
        {
            var endDate = apprenticeship.EndDate.Value.Date;

            var range = plannedStartDate.To(endDate);

            var overlap = await overlapCheckService.CheckForOverlaps(apprenticeship.Uln, range, apprenticeship.Id, CancellationToken.None);

            if (overlap != null)
            {
                return overlap.HasOverlaps;
            }
        }

        return false;
    }

    public async Task<ReservationValidationResult> IsPlannedStartDateValidForReservationAsync(DateTime plannedStartDate, Apprenticeship apprenticeship)
    {
        if (!apprenticeship.ReservationId.HasValue || string.IsNullOrEmpty(apprenticeship.CourseCode))
        {
            return null;
        }

        var validationRequest = new ReservationValidationRequest(apprenticeship.ReservationId.Value, plannedStartDate, apprenticeship.CourseCode);

        var validationResult = await reservationValidationService.Validate(validationRequest, CancellationToken.None);

        return validationResult;
    }

    public bool IsPlannedStartDateLessThanMinAge(DateTime plannedStartDate, DateTime? dateOfBirth)
    {
        return !ageCalculationService.WillLearnerBeAtLeastMinAgeAtStartOfTraining(plannedStartDate, dateOfBirth, Constants.MinimumAgeAtApprenticeshipStart);
    }

    public bool IsPlannedStartDateMoreThanMaxAge(DateTime plannedStartDate, DateTime? dateOfBirth)
    {
        return !ageCalculationService.LearnerAgeMustBeLessThenMaxAgeAtStartOfTraining(plannedStartDate, dateOfBirth, Constants.MaximumAgeAtApprenticeshipStart);
    }

    public bool IsPlannedStartDateMoreThanMaxAgeForLevel7Course(DateTime plannedStartDate, Apprenticeship apprenticeship)
    {
        if (GetStandardCourseLevel(apprenticeship.CourseCode) == Constants.ApprenticeshipCourseLevel7
            && plannedStartDate >= Constants.MaxAgeAt25RequiredOn
            && apprenticeship.Continuation == null)
        {
            return !ageCalculationService.LearnerAgeMustBeLessThenMaxAgeAtStartOfTraining(plannedStartDate, apprenticeship.DateOfBirth, Constants.MaximumAgeAtApprenticeshipStartForLevel7);
        }

        return false;
    }

    public async Task<bool> IsThereEmailOverlapForPlannedStartDateAsync(DateTime plannedStartDate, Apprenticeship apprenticeship)
    {
        if (!string.IsNullOrWhiteSpace(apprenticeship.Email)
            && apprenticeship.StartDate != null
            && apprenticeship.StartDate != plannedStartDate)
        {
            if (apprenticeship.EndDate == null)
            {
                return false;
            }

            var endDate = apprenticeship.EndDate.Value.Date;

            var range = plannedStartDate.To(endDate);

            var overlap = await overlapCheckService.CheckForEmailOverlaps(apprenticeship.Email, range, apprenticeship.Id, apprenticeship.Cohort?.Id, CancellationToken.None);

            if (overlap != null)
            {
                return overlap.OverlapStatus != OverlapStatus.None;
            }
        }

        return false;
    }

    private int? GetStandardCourseLevel(string stdCode)
    {
        if (string.IsNullOrWhiteSpace(stdCode))
        {
            return null;
        }

        return int.TryParse(stdCode, out var result) ? dbContext.Standards.FirstOrDefault(x => x.LarsCode == result)?.LarsCode : null;
    }
}