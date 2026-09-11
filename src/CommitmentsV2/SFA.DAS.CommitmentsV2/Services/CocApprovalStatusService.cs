using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Domain;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Services;

public class CocApprovalStatusService(ILogger<CocApprovalStatusService> logger) : ICocApprovalStatusService
{
    public List<CocUpdateResult> DetermineCocUpdateStatuses(CocApprovalDetails cocApprovalDetails)
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
            if (cocApprovalDetails.ApprovalFieldChanges.Any(afc => afc.ChangeType == nameof(CocChangeField.Firstname)))
            {
                logger.LogInformation("Change of Firstname detected");
                updateResults.Add(DetermineApprovalStatusesForFirstnameField(cocApprovalDetails));
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
        else if (newTotalCost <= oldTotalCost)
        {
            if (updates.TNP1 != null)
            {
                yield return new CocUpdateResult { Field = CocChangeField.TNP1, Status = CocApprovalItemStatus.AutoApproved };
            }
            if (updates.TNP2 != null)
            {
                yield return new CocUpdateResult { Field = CocChangeField.TNP2, Status = CocApprovalItemStatus.AutoApproved };
            }
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

    private CocUpdateResult DetermineApprovalStatusesForFirstnameField(CocApprovalDetails cocApprovalDetails)
    {
        var firstnameChange = cocApprovalDetails.ApprovalFieldChanges.FirstOrDefault(afc => afc.ChangeType == nameof(CocChangeField.Firstname));

        if (firstnameChange?.Data?.Old != cocApprovalDetails.Apprenticeship.FirstName)
        {
            logger.LogWarning("Old first name value from changes, does not match apprenticeship first name value");
        }

        return new CocUpdateResult { Field = CocChangeField.Firstname, Status = CocApprovalItemStatus.AutoApproved };
    }
}