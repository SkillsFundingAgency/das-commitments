using SFA.DAS.CommitmentsV2.Domain.Entities.Reservations;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Validation.CocApprovals.Interfaces;

public interface IPlannedStartDateValidationRules
{
    bool IsPlannedStartDateBeforeAbsoluteMinimum(DateTime plannedStartDate);
    bool IsPlannedStartDateAfterMaximum(DateTime plannedStartDate);
    bool IsPlannedStartDateBeforeFundingWindowHasClosed(DateTime plannedStartDate);
    bool IsPlannedStartDateBeforeLarsEffectiveFrom(DateTime plannedStartDate, Course course);
    bool IsPlannedStartDateAfterLarsEffectiveTo(DateTime plannedStartDate, Course course);
    bool IsPlannedStartDateBeforeTransferFundedDate(DateTime plannedStartDate, Cohort cohort);
    Task<bool> IsPlannedStartDateOverlappingWithUlnDateRangeAsync(DateTime plannedStartDate, Apprenticeship apprenticeship);
    Task<ReservationValidationResult> IsPlannedStartDateValidForReservationAsync(DateTime plannedStartDate, Apprenticeship apprenticeship);
    bool IsPlannedStartDateLessThanMinAge(DateTime plannedStartDate, DateTime? dateOfBirth);
    bool IsPlannedStartDateMoreThanMaxAge(DateTime plannedStartDate, DateTime? dateOfBirth);
    bool IsPlannedStartDateMoreThanMaxAgeForLevel7Course(DateTime plannedStartDate, Apprenticeship apprenticeship, Course course);
    Task<bool> IsThereEmailOverlapForPlannedStartDateAsync(DateTime plannedStartDate, Apprenticeship apprenticeship);
}