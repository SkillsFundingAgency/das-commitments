using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Domain;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Services;

public class CocApprovalStatusService(IOverlapCheckService overlapCheckService, ILogger<CocApprovalStatusService> logger) : ICocApprovalStatusService
{
    public async Task<List<CocUpdateResult>> DetermineCocUpdateStatuses(CocUpdates updates, Apprenticeship apprenticeship)
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

        if (updates.Lastname != null)
        {
            logger.LogInformation("Change of Lastname detected");
            updateResults.Add(DetermineApprovalStatusesForLastnameField(updates, apprenticeship));
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

    private CocUpdateResult DetermineApprovalStatusesForFirstnameField(CocUpdates updates, Apprenticeship apprenticeship)
    {
        if (updates.Firstname.Old != apprenticeship.FirstName)
        {
            logger.LogWarning("Old first name value from changes, does not match apprenticeship first name value");
        }

        return new CocUpdateResult { Field = CocChangeField.Firstname, Status = CocApprovalItemStatus.AutoApproved };
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