using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Extensions;

namespace SFA.DAS.CommitmentsV2.Services;

public class CocApprovalRules(
    ICocApprovalStatusService cocApprovalService,
    ILogger<CocApprovalRules> logger) : ICocApprovalRules
{
    public CocApprovalState DetermineApprovalState(CocApprovalDetails cocApprovalDetails)
    {
        logger.LogInformation("");

        var updateStatuses = cocApprovalService.DetermineCocUpdateStatuses(cocApprovalDetails.Updates, cocApprovalDetails.Apprenticeship);
        var approvalRequestStatus = DetermineApprovalRequestStatus(updateStatuses);
        IEnumerable<ApprovalFieldRequest> approvalFieldRequests = MapToApprovalFieldRequests(cocApprovalDetails, updateStatuses);

        return new CocApprovalState
        {
            ApprovalRequest = new ApprovalRequest
            {
                LearningKey = cocApprovalDetails.LearningKey,
                ApprenticeshipId = cocApprovalDetails.ApprenticeshipId,
                LearningType = cocApprovalDetails.LearningType,
                UKPRN = cocApprovalDetails.ProviderId.ToString(),
                ULN = cocApprovalDetails.ULN,
                Status = approvalRequestStatus,
                Items = approvalFieldRequests.ToList()
            },
            ApprovalResult = new CocApprovalResult
            {
                Status = approvalRequestStatus,
                Items = updateStatuses
            }
        };
    }
    private static IEnumerable<ApprovalFieldRequest> MapToApprovalFieldRequests(CocApprovalDetails command, List<CocUpdateResult> updateStatuses)
    {
        return command.ApprovalFieldChanges.Join(
            updateStatuses,
            change => change.ChangeType,
            status => status.Field.GetEnumDescription(),
            (change, status) => new ApprovalFieldRequest
            {
                Field = change.ChangeType,
                Old = change.Data.Old,
                New = change.Data.New,
                Status = status.Status,
                Reason = status.Reason
            }
            );
    }

    private CocApprovalResultStatus DetermineApprovalRequestStatus(List<CocUpdateResult> updateResults)
    {
        if (updateResults.Any(x => x.Status == CocApprovalItemStatus.Pending))
            return CocApprovalResultStatus.Pending;

        return CocApprovalResultStatus.Complete;
    }
}

public class CocApprovalState
{
    public ApprovalRequest ApprovalRequest { get; set; }
    public CocApprovalResult ApprovalResult { get; set; }
}