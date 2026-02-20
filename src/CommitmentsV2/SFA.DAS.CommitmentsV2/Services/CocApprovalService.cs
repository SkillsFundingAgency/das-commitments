using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Services;

public class CocApprovalService(ILogger<CocApprovalService> logger) : ICocApprovalService
{
    public List<CocUpdateResult> DetermineCocUpdateStatuses(CocUpdates updates, Apprenticeship apprenticeship)
    {
        var updateResults = new List<CocUpdateResult>();

        if (updates == null)
        {
            throw new ArgumentNullException(nameof(updates));
        }

        if(apprenticeship == null)
        {
            throw new ArgumentNullException(nameof(apprenticeship));
        }

        if(updates.TNP1 != null || updates.TNP2 != null)
        {
            logger.LogInformation("Change of TNP1 or TNP2 detected");
            updateResults.AddRange(DetermineApprovalStatusesForCostFields(updates, apprenticeship));
        }

        return updateResults;
    }

    private IEnumerable<CocUpdateResult> DetermineApprovalStatusesForCostFields(CocUpdates updates, Apprenticeship apprenticeship)
    {
        var oldTotalCost = (updates.TNP1?.Old ?? 0) + (updates.TNP2?.Old ?? 0);
        var newTotalCost = (updates.TNP1?.New ?? 0) + (updates.TNP2?.New ?? 0);

        if (oldTotalCost != apprenticeship.Cost)
        {
            logger.LogWarning("Old total cost from changes does not match apprenticeship cost");
        }

        if (newTotalCost <= oldTotalCost)
        {
            if(updates.TNP1 != null)
            {
                yield return new CocUpdateResult { Field = CocChangeField.TNP1, Status = CocApprovalItemStatus.AutoApproved };
            }
            if (updates.TNP2 != null)
            {
                yield return new CocUpdateResult { Field = CocChangeField.TNP2, Status = CocApprovalItemStatus.AutoApproved };
            }
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
}