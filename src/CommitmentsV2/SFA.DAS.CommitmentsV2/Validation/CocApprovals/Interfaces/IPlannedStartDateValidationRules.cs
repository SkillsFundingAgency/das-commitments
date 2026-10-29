using SFA.DAS.CommitmentsV2.Domain.Entities.Reservations;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Validation.CocApprovals.Interfaces;

public interface ICocApprovalStartDateValidationRules
{
    bool IsStartDateBeforeAbsoluteMinimum(DateTime startDate);
    bool IsStartDateAfterMaximum(DateTime startDate);
    bool IsStartDateBeforeFundingWindowHasClosed(DateTime startDate);
    bool IsActualStartDateBeforeLarsEffectiveFrom(DateTime actualStartDate, Course course);
    bool IsActualStartDateAfterLarsEffectiveTo(DateTime actualStartDate, Course course);
    bool IsStartDateBeforeTransferFundedDate(DateTime startDate, Cohort cohort);
    Task<bool> IsStartDateOverlappingWithUlnDateRangeAsync(DateTime startDate, Apprenticeship apprenticeship);
    Task<ReservationValidationResult> IsStartDateValidForReservationAsync(DateTime startDate, Apprenticeship apprenticeship);
    bool IsStartDateLessThanMinAge(DateTime startDate, DateTime? dateOfBirth);
    bool IsStartDateMoreThanMaxAge(DateTime startDate, DateTime? dateOfBirth);
    bool IsStartDateMoreThanMaxAgeForLevel7Course(DateTime startDate, Apprenticeship apprenticeship, Course course);
    Task<bool> IsThereEmailOverlapForStartDateAsync(DateTime startDate, Apprenticeship apprenticeship);
}