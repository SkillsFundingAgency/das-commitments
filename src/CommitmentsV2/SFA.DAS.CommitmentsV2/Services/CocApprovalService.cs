using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Services;

public class CocApprovalService(ILogger<CocApprovalService> logger) : ICocApprovalService
{
    public CocApprovalRequestStatus DetermineAndSetCocApprovalStatuses(CocChanges changes, Apprenticeship apprenticeship)
    {
        if(changes == null)
        {
            throw new ArgumentNullException(nameof(changes));
        }

        if(apprenticeship == null)
        {
            throw new ArgumentNullException(nameof(apprenticeship));
        }

        if(changes.TNP1 != null || changes.TNP2 != null)
        {
            logger.LogInformation("Change of TNP1 or TNP2 detected");
            SetCocApprovalStatusesForCosts(changes, apprenticeship);
        }

        return DetermineApprovalRequestStatus(changes);
    }

    private void SetCocApprovalStatusesForCosts(CocChanges changes, Apprenticeship apprenticeship)
    {
        var oldTotalCost = changes.TNP1?.Old ?? 0 + changes.TNP2?.Old ?? 0;
        var newTotalCost = changes.TNP1?.New ?? 0 + changes.TNP2?.New ?? 0;

        if (oldTotalCost != apprenticeship.Cost)
        {
            logger.LogWarning("Old total cost from changes does not match apprenticeship cost");
        }

        if (newTotalCost <= oldTotalCost)
        {
            if(changes.TNP1 != null)
            {
                changes.TNP1.Status = CocApprovalItemStatus.AutoApproved;
            }
            if (changes.TNP2 != null)
            {
                changes.TNP2.Status = CocApprovalItemStatus.AutoApproved;
            }
        }
        else
        {
            if (changes.TNP1 != null)
            {
                changes.TNP1.Status = CocApprovalItemStatus.Pending;
            }
            if (changes.TNP2 != null)
            {
                changes.TNP2.Status = CocApprovalItemStatus.Pending;
            }

        }
    }

    private CocApprovalRequestStatus DetermineApprovalRequestStatus(CocChanges changes)
    {
        if (changes.TNP1?.Status == CocApprovalItemStatus.Pending)
            return CocApprovalRequestStatus.Pending;
        if (changes.TNP2?.Status == CocApprovalItemStatus.Pending)
            return CocApprovalRequestStatus.Pending;

        return CocApprovalRequestStatus.Complete;
    }
}