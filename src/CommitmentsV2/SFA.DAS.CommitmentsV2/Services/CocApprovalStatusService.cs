using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Domain;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Extensions;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Validation.CocApprovals.Interfaces;

namespace SFA.DAS.CommitmentsV2.Services;

public class CocApprovalStatusService(
        IOverlapCheckService overlapCheckService,
        IPlannedStartDateValidationRules plannedStartDateValidationRules,
        IAcademicYearDateProvider academicYearDateProvider,
        ILogger<CocApprovalStatusService> logger)
    : ICocApprovalStatusService
{
    public async Task<List<CocUpdateResult>> DetermineCocUpdateStatusesAsync(CocUpdates updates, Apprenticeship apprenticeship, Course course)
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

        if (apprenticeship.StopDate != null)
        {
            updateResults.AddRange(AutoRejectFieldsAffectedByStoppedApprenticeship(updates));
        }
        else
        {
            if (updates.PlannedEndDate != null)
            {
                logger.LogInformation("Change of PlannedEndDate detected");
                var list = await DetermineStatusOfCourseDates(updates, apprenticeship).ToListAsync();
                updateResults.AddRange(list);
            }
        }

        if (updates.Firstname != null)
        {
            logger.LogInformation("Change of Firstname detected");
            updateResults.Add(DetermineApprovalStatusesForFirstnameField(updates, apprenticeship));
        }

        if (updates.PlannedStartDate != null)
        {
            logger.LogInformation("Change of PlannedStartDate detected");
            updateResults.Add(await DetermineApprovalStatusesForPlannedStartDateFieldAsync(updates, apprenticeship, course));
        }

        if (updates.TNP1 != null || updates.TNP2 != null)
        {
            logger.LogInformation("Change of TNP1 or TNP2 detected");
            updateResults.AddRange(DetermineApprovalStatusesForCostFields(updates, apprenticeship));
        }

        return updateResults;
    }

    private IEnumerable<CocUpdateResult> AutoRejectFieldsAffectedByStoppedApprenticeship(CocUpdates updates)
    {
        if (updates.PlannedEndDate != null)
        {
            yield return new CocUpdateResult { Field = CocChangeField.PlannedEndDate, Status = CocApprovalItemStatus.AutoRejected, Reason = "The end date cannot be changed on a stopped record" };
        }
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

            if (apprenticeship.PaymentStatus == Types.PaymentStatus.Completed && apprenticeship.CompletionDate.HasValue && newPlannedEndDate > apprenticeship.CompletionDate)
            {
                yield return new CocUpdateResult { Field = CocChangeField.PlannedEndDate, Status = CocApprovalItemStatus.AutoRejected, Reason = "The end date cannot be changed on a completed record" };
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

    private IEnumerable<CocUpdateResult> DetermineApprovalStatusesForCostFields(CocUpdates updates, Apprenticeship apprenticeship)
    {
        var oldTotalCost = (updates.TNP1?.Old ?? 0) + (updates.TNP2?.Old ?? 0);
        var newTotalCost = (updates.TNP1?.New ?? 0) + (updates.TNP2?.New ?? 0);

        if (oldTotalCost != apprenticeship.Cost)
        {
            // TODO raise concerns, that we are ignoring this mismatch
            logger.LogWarning("Old total cost from changes does not match apprenticeship cost");
        }

        if (updates.TNP1?.New == 0)
        {
            yield return new CocUpdateResult { Field = CocChangeField.TNP1, Status = CocApprovalItemStatus.AutoRejected };
            yield return new CocUpdateResult { Field = CocChangeField.TNP2, Status = CocApprovalItemStatus.AutoRejected };
        }
        else if (newTotalCost > Constants.MaximumTotalTrainingCost)
        {
            yield return new CocUpdateResult { Field = CocChangeField.TNP2, Status = CocApprovalItemStatus.AutoRejected };
            yield return new CocUpdateResult { Field = CocChangeField.TNP1, Status = CocApprovalItemStatus.AutoRejected };
        }
        else
        {
            if (updates.TNP1 != null)
            {
                yield return new CocUpdateResult { Field = CocChangeField.TNP1, Status = CocApprovalItemStatus.Pending };
            }
            if (updates.TNP2 != null)
            {
                yield return new CocUpdateResult { Field = CocChangeField.TNP2, Status = CocApprovalItemStatus.Pending };
            }
        }
    }

    private CocUpdateResult DetermineApprovalStatusesForFirstnameField(CocUpdates updates, Apprenticeship apprenticeship)
    {
        if (updates.Firstname.Old != apprenticeship.FirstName)
        {
            logger.LogWarning("Old first name value from changes, does not match apprenticeship first name value");
        }
        return new CocUpdateResult { Field = CocChangeField.Firstname, Status = CocApprovalItemStatus.AutoApproved };
    }

    private async Task<CocUpdateResult> DetermineApprovalStatusesForPlannedStartDateFieldAsync(CocUpdates updates, Apprenticeship apprenticeship, Course course)
    {
        var plannedStartDateNew = updates.PlannedStartDate.New ?? DateTime.MinValue;
        var reservationValidationResult = await plannedStartDateValidationRules.IsPlannedStartDateValidForReservationAsync(plannedStartDateNew, apprenticeship);

        if (plannedStartDateValidationRules.IsPlannedStartDateBeforeAbsoluteMinimum(plannedStartDateNew))
        {
            return new CocUpdateResult { Field = CocChangeField.PlannedStartDate, Status = CocApprovalItemStatus.AutoRejected, Reason = "The start date must not be earlier than May 2017" };
        }
        else if (plannedStartDateValidationRules.IsPlannedStartDateAfterMaximum(plannedStartDateNew))
        {
            return new CocUpdateResult { Field = CocChangeField.PlannedStartDate, Status = CocApprovalItemStatus.AutoRejected, Reason = "The start date must be no later than one year after the end of the current teaching year" };
        }
        else if (plannedStartDateValidationRules.IsPlannedStartDateBeforeFundingWindowHasClosed(plannedStartDateNew))
        {
            return new CocUpdateResult { Field = CocChangeField.PlannedStartDate, Status = CocApprovalItemStatus.AutoRejected, Reason = $"The earliest start date you can use is {academicYearDateProvider.CurrentAcademicYearStartDate.ToGdsFormatShortMonthWithoutDay()}" };
        }
        else if (plannedStartDateValidationRules.IsPlannedStartDateBeforeLarsEffectiveFrom(plannedStartDateNew, course))
        {
            var previousMonth = course?.EffectiveFrom.Value.AddMonths(-1);
            return new CocUpdateResult { Field = CocChangeField.PlannedStartDate, Status = CocApprovalItemStatus.AutoRejected, Reason = $"This training course is only available to learners with a start date after {previousMonth.Value.Month} {previousMonth.Value.Year}" };
        }
        else if (plannedStartDateValidationRules.IsPlannedStartDateAfterLarsEffectiveTo(plannedStartDateNew, course))
        {
            var nextMonth = course?.EffectiveTo.Value.AddMonths(1);
            return new CocUpdateResult { Field = CocChangeField.PlannedStartDate, Status = CocApprovalItemStatus.AutoRejected, Reason = $"This training course is only available to learners with a start date before {nextMonth.Value.Month} {nextMonth.Value.Year}" };
        }
        else if (plannedStartDateValidationRules.IsPlannedStartDateBeforeTransferFundedDate(plannedStartDateNew, apprenticeship.Cohort))
        {
            return new CocUpdateResult { Field = CocChangeField.PlannedStartDate, Status = CocApprovalItemStatus.AutoRejected, Reason = "Learners funded through a transfer can't start earlier than May 2018" };
        }
        else if (await plannedStartDateValidationRules.IsPlannedStartDateOverlappingWithUlnDateRangeAsync(plannedStartDateNew, apprenticeship))
        {
            return new CocUpdateResult { Field = CocChangeField.PlannedStartDate, Status = CocApprovalItemStatus.AutoRejected, Reason = "Learners overlapping dates and therefore cant start" };
        }
        else if (reservationValidationResult != null && reservationValidationResult.HasErrors)
        {
            return new CocUpdateResult { Field = CocChangeField.PlannedStartDate, Status = CocApprovalItemStatus.AutoRejected, Reason = $"{reservationValidationResult.ValidationErrors?.FirstOrDefault()?.Reason}" };
        }
        else if (plannedStartDateValidationRules.IsPlannedStartDateLessThanMinAge(plannedStartDateNew, apprenticeship.DateOfBirth))
        {
            return new CocUpdateResult { Field = CocChangeField.PlannedStartDate, Status = CocApprovalItemStatus.AutoRejected, Reason = $"The learner must be at least {Constants.MinimumAgeAtApprenticeshipStart} years old at the start of their training" };
        }
        else if (plannedStartDateValidationRules.IsPlannedStartDateMoreThanMaxAge(plannedStartDateNew, apprenticeship.DateOfBirth))
        {
            return new CocUpdateResult { Field = CocChangeField.PlannedStartDate, Status = CocApprovalItemStatus.AutoRejected, Reason = $"The learner must be younger than {Constants.MaximumAgeAtApprenticeshipStart} years old at the start of their training" };
        }
        else if (plannedStartDateValidationRules.IsPlannedStartDateMoreThanMaxAgeForLevel7Course(plannedStartDateNew, apprenticeship, course))
        {
            return new CocUpdateResult { Field = CocChangeField.PlannedStartDate, Status = CocApprovalItemStatus.AutoRejected, Reason = $"The learner must be younger than {Constants.MaximumAgeAtApprenticeshipStartForLevel7} years old at the start of their training" };
        }
        else if (await plannedStartDateValidationRules.IsThereEmailOverlapForPlannedStartDateAsync(plannedStartDateNew, apprenticeship))
        {
            return new CocUpdateResult { Field = CocChangeField.PlannedStartDate, Status = CocApprovalItemStatus.AutoRejected, Reason = "This email address is already used for another apprenticeship in the same training period - use a different email address" };
        }

        return new CocUpdateResult { Field = CocChangeField.PlannedStartDate, Status = CocApprovalItemStatus.AutoApproved };
    }
}