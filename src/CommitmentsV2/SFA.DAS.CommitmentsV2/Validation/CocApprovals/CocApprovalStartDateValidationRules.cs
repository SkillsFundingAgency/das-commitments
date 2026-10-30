using SFA.DAS.CommitmentsV2.Domain;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Entities.Reservations;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Validation.CocApprovals.Interfaces;

namespace SFA.DAS.CommitmentsV2.Validation.CocApprovals;

public class CocApprovalStartDateValidationRules(
    IAcademicYearDateProvider academicYearDateProvider,
    ICurrentDateTime currentDateTime,
    IAgeCalculationService ageCalculationService,
    IOverlapCheckService overlapCheckService,
    IReservationValidationService reservationValidationService) 
    :ICocApprovalStartDateValidationRules
{
    public bool IsStartDateBeforeAbsoluteMinimum(DateTime startDate)
    {
        return startDate < Constants.DasStartDate;
    }
    public bool IsStartDateAfterMaximum(DateTime startDate)
    {
        return startDate > academicYearDateProvider.CurrentAcademicYearEndDate.AddYears(1);
    }

    public bool IsStartDateBeforeFundingWindowHasClosed(DateTime startDate)
    {
        return (currentDateTime.UtcNow > academicYearDateProvider.LastAcademicYearFundingPeriod) && (startDate < academicYearDateProvider.CurrentAcademicYearStartDate);
    }

    public bool IsActualStartDateBeforeLarsEffectiveFrom(DateTime actualStartDate, Course course)
    {
        return actualStartDate < course?.EffectiveFrom;
    }

    public bool IsActualStartDateAfterLarsEffectiveTo(DateTime actualStartDate, Course course)
    {
        return actualStartDate > course?.EffectiveTo;
    }

    public bool IsStartDateBeforeTransferFundedDate(DateTime startDate, Cohort cohort)
    {
        if (cohort != null)
        {
            return cohort.TransferSenderId.HasValue && startDate < Constants.TransferFeatureStartDate;
        }

        return false;
    }

    public async Task<bool> IsStartDateOverlappingWithUlnDateRangeAsync(DateTime startDate, Apprenticeship apprenticeship)
    {
        if (apprenticeship.EndDate != null)
        {
            var endDate = apprenticeship.EndDate.Value.Date;

            var range = startDate.To(endDate);

            var overlap = await overlapCheckService.CheckForOverlaps(apprenticeship.Uln, range, apprenticeship.Id, CancellationToken.None);

            if (overlap != null)
            {
                return overlap.HasOverlaps;
            }
        }

        return false;
    }

    public async Task<ReservationValidationResult> IsStartDateValidForReservationAsync(DateTime startDate, Apprenticeship apprenticeship)
    {
        if (!apprenticeship.ReservationId.HasValue || string.IsNullOrEmpty(apprenticeship.CourseCode))
        {
            return null;
        }

        var validationRequest = new ReservationValidationRequest(apprenticeship.ReservationId.Value, startDate, apprenticeship.CourseCode);

        var validationResult = await reservationValidationService.Validate(validationRequest, CancellationToken.None);

        return validationResult;
    }

    public bool IsStartDateLessThanMinAge(DateTime startDate, DateTime? dateOfBirth)
    {
        return !ageCalculationService.WillLearnerBeAtLeastMinAgeAtStartOfTraining(startDate, dateOfBirth, Constants.MinimumAgeAtApprenticeshipStart);
    }

    public bool IsStartDateMoreThanMaxAge(DateTime startDate, DateTime? dateOfBirth)
    {
        return !ageCalculationService.LearnerAgeMustBeLessThenMaxAgeAtStartOfTraining(startDate, dateOfBirth, Constants.MaximumAgeAtApprenticeshipStart);
    }

    public bool IsStartDateMoreThanMaxAgeForLevel7Course(DateTime startDate, Apprenticeship apprenticeship, Course course)
    {
        if (course.Level == Constants.ApprenticeshipCourseLevel7.ToString()
            && startDate >= Constants.MaxAgeAt25RequiredOn
            && apprenticeship.Continuation == null)
        {
            return !ageCalculationService.LearnerAgeMustBeLessThenMaxAgeAtStartOfTraining(startDate, apprenticeship.DateOfBirth, Constants.MaximumAgeAtApprenticeshipStartForLevel7);
        }

        return false;
    }

    public async Task<bool> IsThereEmailOverlapForStartDateAsync(DateTime startDate, Apprenticeship apprenticeship)
    {
        if (!string.IsNullOrWhiteSpace(apprenticeship.Email)
            && apprenticeship.StartDate != null
            && apprenticeship.StartDate != startDate
            && apprenticeship.EmailAddressConfirmed != true)
        {
            if (apprenticeship.EndDate == null)
            {
                return false;
            }

            var endDate = apprenticeship.EndDate.Value.Date;

            var range = startDate.To(endDate);

            var overlap = await overlapCheckService.CheckForEmailOverlaps(apprenticeship.Email, range, apprenticeship.Id, apprenticeship.Cohort?.Id, CancellationToken.None);

            if (overlap != null)
            {
                return overlap.OverlapStatus != OverlapStatus.None;
            }
        }

        return false;
    }
}