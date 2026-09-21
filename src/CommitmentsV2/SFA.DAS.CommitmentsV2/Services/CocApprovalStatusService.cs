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
    IPlannedStartDateValidationRules plannedStartDateValidationRules,
    IAcademicYearDateProvider academicYearDateProvider,
    ILogger<CocApprovalStatusService> logger) 
    : ICocApprovalStatusService
{
    public async Task<List<CocUpdateResult>> DetermineCocUpdateStatusesAsync(CocApprovalDetails cocApprovalDetails)
    {
        var updateResults = new List<CocUpdateResult>();

        if (cocApprovalDetails.Updates == null)
        {
            throw new ArgumentNullException(nameof(cocApprovalDetails.Updates));
        }

        if (cocApprovalDetails.Apprenticeship == null)
        {
            throw new ArgumentNullException(nameof(cocApprovalDetails.Apprenticeship));
        }

        if (cocApprovalDetails.ApprovalFieldChanges != null)
        {
            if (cocApprovalDetails.ApprovalFieldChanges.Any(afc => afc.ChangeType == nameof(CocChangeField.PlannedStartDate)))
            {
                logger.LogInformation("Change of PlannedStartDate detected");
                updateResults.Add(await DetermineApprovalStatusesForPlannedStartDateFieldAsync(cocApprovalDetails));
            }
        }

        if (cocApprovalDetails.Updates.TNP1 != null || cocApprovalDetails.Updates.TNP2 != null)
        {
            logger.LogInformation("Change of TNP1 or TNP2 detected");
            updateResults.AddRange(DetermineApprovalStatusesForCostFields(cocApprovalDetails.Updates, cocApprovalDetails.Apprenticeship));
        }

        return updateResults;
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

    private async Task<CocUpdateResult> DetermineApprovalStatusesForPlannedStartDateFieldAsync(CocApprovalDetails cocApprovalDetails)
    {
        var plannedStartDateChange = cocApprovalDetails.ApprovalFieldChanges.FirstOrDefault(afc => afc.ChangeType == nameof(CocChangeField.PlannedStartDate));
        var plannedStartDateNew = Convert.ToDateTime(plannedStartDateChange?.Data?.New);

        var reservationValidationResult = await plannedStartDateValidationRules.IsPlannedStartDateValidForReservationAsync(plannedStartDateNew, cocApprovalDetails.Apprenticeship);

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
        else if (plannedStartDateValidationRules.IsPlannedStartDateBeforeLarsEffectiveFrom(plannedStartDateNew, cocApprovalDetails.Course))
        {
            var previousMonth = cocApprovalDetails.Course?.EffectiveFrom.Value.AddMonths(-1);
            return new CocUpdateResult { Field = CocChangeField.PlannedStartDate, Status = CocApprovalItemStatus.AutoRejected, Reason = $"This training course is only available to learners with a start date after {previousMonth.Value.Month} {previousMonth.Value.Year}"};
        }
        else if (plannedStartDateValidationRules.IsPlannedStartDateAfterLarsEffectiveTo(plannedStartDateNew, cocApprovalDetails.Course))
        {
            var nextMonth = cocApprovalDetails.Course?.EffectiveTo.Value.AddMonths(1);
            return new CocUpdateResult { Field = CocChangeField.PlannedStartDate, Status = CocApprovalItemStatus.AutoRejected, Reason = $"This training course is only available to learners with a start date before {nextMonth.Value.Month} {nextMonth.Value.Year}" };
        }
        else if (plannedStartDateValidationRules.IsPlannedStartDateBeforeTransferFundedDate(plannedStartDateNew, cocApprovalDetails.Apprenticeship.Cohort))
        {
            return new CocUpdateResult { Field = CocChangeField.PlannedStartDate, Status = CocApprovalItemStatus.AutoRejected, Reason = "Learners funded through a transfer can't start earlier than May 2018" };
        }
        else if (await plannedStartDateValidationRules.IsPlannedStartDateOverlappingWithUlnDateRangeAsync(plannedStartDateNew, cocApprovalDetails.Apprenticeship))
        {
            return new CocUpdateResult { Field = CocChangeField.PlannedStartDate, Status = CocApprovalItemStatus.AutoRejected, Reason = "Learners overlapping dates and therefore cant start" };
        }
        else if (reservationValidationResult != null && reservationValidationResult.HasErrors)
        {
            return new CocUpdateResult { Field = CocChangeField.PlannedStartDate, Status = CocApprovalItemStatus.AutoRejected, Reason = $"{reservationValidationResult.ValidationErrors?.FirstOrDefault()?.Reason}" };
        }
        else if (plannedStartDateValidationRules.IsPlannedStartDateLessThanMinAge(plannedStartDateNew, cocApprovalDetails.Apprenticeship.DateOfBirth))
        {
            return new CocUpdateResult { Field = CocChangeField.PlannedStartDate, Status = CocApprovalItemStatus.AutoRejected, Reason = $"The learner must be at least {Constants.MinimumAgeAtApprenticeshipStart} years old at the start of their training" };
        }
        else if (plannedStartDateValidationRules.IsPlannedStartDateMoreThanMaxAge(plannedStartDateNew, cocApprovalDetails.Apprenticeship.DateOfBirth))
        {
            return new CocUpdateResult { Field = CocChangeField.PlannedStartDate, Status = CocApprovalItemStatus.AutoRejected, Reason = $"The learner must be younger than {Constants.MaximumAgeAtApprenticeshipStart} years old at the start of their training" };
        }
        else if (plannedStartDateValidationRules.IsPlannedStartDateMoreThanMaxAgeForLevel7Course(plannedStartDateNew, cocApprovalDetails.Apprenticeship, cocApprovalDetails.Course))
        {
            return new CocUpdateResult { Field = CocChangeField.PlannedStartDate, Status = CocApprovalItemStatus.AutoRejected, Reason = $"The learner must be younger than {Constants.MaximumAgeAtApprenticeshipStartForLevel7} years old at the start of their training" };
        }
        else if (await plannedStartDateValidationRules.IsThereEmailOverlapForPlannedStartDateAsync(plannedStartDateNew, cocApprovalDetails.Apprenticeship))
        {
            return new CocUpdateResult { Field = CocChangeField.PlannedStartDate, Status = CocApprovalItemStatus.AutoRejected, Reason = "This email address is already used for another apprenticeship in the same training period - use a different email address" };
        }

        return new CocUpdateResult { Field = CocChangeField.PlannedStartDate, Status = CocApprovalItemStatus.AutoApproved };
    }
}