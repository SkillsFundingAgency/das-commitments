using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Domain;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Extensions;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.CommitmentsV2.Validation.CocApprovals.Interfaces;

namespace SFA.DAS.CommitmentsV2.Services;

public class CocApprovalStatusService(
        IOverlapCheckService overlapCheckService,
        ICocApprovalStartDateValidationRules cocApprovalStartDateValidationRules,
        IAcademicYearDateProvider academicYearDateProvider,
        ILogger<CocApprovalStatusService> logger)
    : ICocApprovalStatusService
{
    public async Task<List<CocUpdateResult>> DetermineCocUpdateStatuses(CocUpdates updates, Apprenticeship apprenticeship, Course course)
    {
        var updateResults = new List<CocUpdateResult>();

        if (updates == null)
        {
            throw new ArgumentNullException(nameof(updates));
        }

        if (apprenticeship == null)
        {
            throw new ArgumentNullException(nameof(apprenticeship));
        }

        if (course == null)
        {
            throw new ArgumentNullException(nameof(course));
        }

        if (updates.PlannedEndDate != null)
        {
            logger.LogInformation("Change of PlannedEndDate detected");
            var list = await DetermineStatusOfCourseDates(updates, apprenticeship).ToListAsync();
            updateResults.AddRange(list);
        }

        if (updates.Firstname != null)
        {
            logger.LogInformation("Change of Firstname detected");
            updateResults.Add(DetermineApprovalStatusesForFirstnameField(updates, apprenticeship));
        }

        if (updates.Lastname != null)
        {
            logger.LogInformation("Change of Lastname detected");
            updateResults.Add(DetermineApprovalStatusesForLastnameField(updates, apprenticeship));
        }

        if (updates.StartDate != null)
        {
            logger.LogInformation("Change of StartDate detected");
            updateResults.Add(await DetermineApprovalStatusesForCocApprovalStartDateField(updates, apprenticeship, course));
        }

        return updateResults;
    }

    private async IAsyncEnumerable<CocUpdateResult> DetermineStatusOfCourseDates(CocUpdates updates, Apprenticeship apprenticeship)
    {
        if (updates.PlannedEndDate != null)
        {
            if (updates.PlannedEndDate.Old != apprenticeship.EndDate)
            {
                logger.LogWarning("Old planned end date from changes does not match apprenticeship end date");
            }

            var newPlannedEndDate = CreateDateAsFirstOfMonth(updates.PlannedEndDate.New.Value);

            if (apprenticeship.EndDate.HasValue && newPlannedEndDate == CreateDateAsFirstOfMonth(apprenticeship.EndDate.Value))
            {
                yield return new CocUpdateResult { Field = CocChangeField.PlannedEndDate, Status = CocApprovalItemStatus.Reversal };
            }
            else if (newPlannedEndDate < Constants.DasStartDate)
            {
                yield return new CocUpdateResult { Field = CocChangeField.PlannedEndDate, Status = CocApprovalItemStatus.AutoRejected, Reason = "The end date must not be earlier than May 2017" };
            }
            else if (newPlannedEndDate < apprenticeship.StartDate)
            {
                yield return new CocUpdateResult { Field = CocChangeField.PlannedEndDate, Status = CocApprovalItemStatus.AutoRejected, Reason = "The end date must not be before the start date" };
            }
            else if (apprenticeship.FlexibleEmployment != null && newPlannedEndDate < apprenticeship.FlexibleEmployment.EmploymentEndDate)
            {
                yield return new CocUpdateResult { Field = CocChangeField.PlannedEndDate, Status = CocApprovalItemStatus.AutoRejected, Reason = "The end date must not be earlier than FlexibleEmployment.EmploymentEndDate" };
            }
            else if (await CheckForUlnOverlaps(newPlannedEndDate, apprenticeship))
            {
                yield return new CocUpdateResult { Field = CocChangeField.PlannedEndDate, Status = CocApprovalItemStatus.AutoRejected, Reason = "Apprentices overlapping dates and therefore cant start" };
            }
            else
            {
                yield return new CocUpdateResult { Field = CocChangeField.PlannedEndDate, Status = CocApprovalItemStatus.Pending };
            }
        }
    }

    private DateTime CreateDateAsFirstOfMonth(DateTime value)
    {
        return new DateTime(value.Year, value.Month, 1);
    }

    private async Task<bool> CheckForUlnOverlaps(DateTime newPlannedEndDate, Apprenticeship apprenticeship)
    {
        var courseDateRange = new Domain.Entities.CourseDateRange(apprenticeship.StartDate.Value, newPlannedEndDate);
        var overlap = await overlapCheckService.CheckForOverlaps(apprenticeship.Uln, courseDateRange, apprenticeship.Id, default);
        return overlap.HasOverlaps;
    }

    private CocUpdateResult DetermineApprovalStatusesForFirstnameField(CocUpdates updates, Apprenticeship apprenticeship)
    {
        if (updates.Firstname.Old != apprenticeship.FirstName)
        {
            logger.LogWarning("Old first name value from changes, does not match apprenticeship first name value");
        }
        return new CocUpdateResult { Field = CocChangeField.Firstname, Status = CocApprovalItemStatus.AutoApproved };
    }

    private async Task<CocUpdateResult> DetermineApprovalStatusesForCocApprovalStartDateField(CocUpdates updates, Apprenticeship apprenticeship, Course course)
    {
        // Validation is on StartDate (or StartDate), which is the 1st of the month (01-MM-YYYY) derived from the ActualStartDate coming from ILR.
        // The only exception is when we are comparing for the Course - LARS EffectiveFrom and EffectiveTo dates, which are based on the ActualStartDate (DD-MM-YYYY) coming from ILR. 
        var startDateNew = updates.StartDate.New.HasValue ? new DateTime(updates.StartDate.New.Value.Year, updates.StartDate.New.Value.Month, 1) : DateTime.MinValue;
        var actualStartDateNew = updates.StartDate.New ?? DateTime.MinValue;
        var ageLimits = GetAgeLimits(course);
        var reservationValidationResult = await cocApprovalStartDateValidationRules.IsStartDateValidForReservationAsync(startDateNew, apprenticeship);

        if (cocApprovalStartDateValidationRules.IsStartDateBeforeAbsoluteMinimum(startDateNew))
        {
            return new CocUpdateResult { Field = CocChangeField.StartDate, Status = CocApprovalItemStatus.AutoRejected, Reason = "The start date must not be earlier than May 2017" };
        }
        else if (cocApprovalStartDateValidationRules.IsStartDateAfterMaximum(startDateNew))
        {
            return new CocUpdateResult { Field = CocChangeField.StartDate, Status = CocApprovalItemStatus.AutoRejected, Reason = "The start date must be no later than one year after the end of the current teaching year" };
        }
        else if (cocApprovalStartDateValidationRules.IsStartDateBeforeFundingWindowHasClosed(startDateNew))
        {
            return new CocUpdateResult { Field = CocChangeField.StartDate, Status = CocApprovalItemStatus.AutoRejected, Reason = $"The earliest start date you can use is {academicYearDateProvider.CurrentAcademicYearStartDate.ToGdsFormatShortMonthWithoutDay()}" };
        }
        else if (cocApprovalStartDateValidationRules.IsActualStartDateBeforeLarsEffectiveFrom(actualStartDateNew, course))
        {
            var previousMonth = course?.EffectiveFrom?.AddMonths(-1);
            return new CocUpdateResult { Field = CocChangeField.StartDate, Status = CocApprovalItemStatus.AutoRejected, Reason = $"This training course is only available to learners with a start date after {previousMonth?.ToGdsFormatShortMonthWithoutDay()}" };
        }
        else if (cocApprovalStartDateValidationRules.IsActualStartDateAfterLarsEffectiveTo(actualStartDateNew, course))
        {
            var nextMonth = course?.EffectiveTo?.AddMonths(1);
            return new CocUpdateResult { Field = CocChangeField.StartDate, Status = CocApprovalItemStatus.AutoRejected, Reason = $"This training course is only available to learners with a start date before {nextMonth?.ToGdsFormatShortMonthWithoutDay()}" };
        }
        else if (cocApprovalStartDateValidationRules.IsStartDateBeforeTransferFundedDate(startDateNew, apprenticeship.Cohort))
        {
            return new CocUpdateResult { Field = CocChangeField.StartDate, Status = CocApprovalItemStatus.AutoRejected, Reason = "Learners funded through a transfer can't start earlier than May 2018" };
        }
        else if (await cocApprovalStartDateValidationRules.IsStartDateOverlappingWithUlnDateRangeAsync(startDateNew, apprenticeship))
        {
            return new CocUpdateResult { Field = CocChangeField.StartDate, Status = CocApprovalItemStatus.AutoRejected, Reason = "Learners overlapping dates and therefore cant start" };
        }
        else if (reservationValidationResult != null && reservationValidationResult.HasErrors)
        {
            return new CocUpdateResult { Field = CocChangeField.StartDate, Status = CocApprovalItemStatus.AutoRejected, Reason = $"{reservationValidationResult.ValidationErrors?.FirstOrDefault()?.Reason}" };
        }
        else if (cocApprovalStartDateValidationRules.IsStartDateLessThanMinAge(startDateNew, apprenticeship.DateOfBirth, ageLimits.MinimumAge))
        {
            return new CocUpdateResult { Field = CocChangeField.StartDate, Status = CocApprovalItemStatus.AutoRejected, Reason = $"The learner must be at least {ageLimits.MinimumAge} years old at the start of their training" };
        }
        else if (cocApprovalStartDateValidationRules.IsStartDateMoreThanMaxAge(startDateNew, apprenticeship.DateOfBirth, ageLimits.MaximumAge))
        {
            return new CocUpdateResult { Field = CocChangeField.StartDate, Status = CocApprovalItemStatus.AutoRejected, Reason = $"The learner must be younger than {ageLimits.MaximumAge} years old at the start of their training" };
        }
        else if (cocApprovalStartDateValidationRules.IsStartDateMoreThanMaxAgeForLevel7Course(startDateNew, apprenticeship, course))
        {
            return new CocUpdateResult { Field = CocChangeField.StartDate, Status = CocApprovalItemStatus.AutoRejected, Reason = $"The learner must be younger than {Constants.MaximumAgeAtApprenticeshipStartForLevel7} years old at the start of their training" };
        }
        else if (cocApprovalStartDateValidationRules.IsActualStartDateAfterStopOrCompletionDate(actualStartDateNew, apprenticeship))
        {
            var reason = apprenticeship.PaymentStatus == PaymentStatus.Withdrawn
                ? "The start date must not be after the stop date"
                : "The start date must not be after the completion date";
            return new CocUpdateResult { Field = CocChangeField.StartDate, Status = CocApprovalItemStatus.AutoRejected, Reason = reason };
        }
        else if (await cocApprovalStartDateValidationRules.IsThereEmailOverlapForStartDateAsync(startDateNew, apprenticeship))
        {
            return new CocUpdateResult { Field = CocChangeField.StartDate, Status = CocApprovalItemStatus.AutoRejected, Reason = "This email address is already used for another apprenticeship in the same training period - use a different email address" };
        }

        return new CocUpdateResult { Field = CocChangeField.StartDate, Status = CocApprovalItemStatus.Pending };
    }

    private static (int MinimumAge, int MaximumAge) GetAgeLimits(Course course)
    {
        return course.LearningType switch
        {
            LearningType.FoundationApprenticeship => (Constants.MinimumAgeAtApprenticeshipStart, Constants.MaximumAgeAtApprenticeshipStartForLevel7),
            LearningType.ApprenticeshipUnit => (Constants.MinimumAgeAtShortCourseStart, Constants.MaximumAgeAtApprenticeshipStart),
            _ => (Constants.MinimumAgeAtApprenticeshipStart, Constants.MaximumAgeAtApprenticeshipStart)
        };
    }

    private CocUpdateResult DetermineApprovalStatusesForLastnameField(CocUpdates updates, Apprenticeship apprenticeship)
    {
        if (updates.Lastname.Old != apprenticeship.LastName)
        {
            logger.LogWarning("Old last name value from changes, does not match apprenticeship last name value");
        }

        return new CocUpdateResult { Field = CocChangeField.Lastname, Status = CocApprovalItemStatus.AutoApproved };
    }
}