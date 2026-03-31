using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Extensions;

namespace SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeship;

public class PostCocApprovalCommandHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ICocApprovalService cocApprovalService,
    ILogger<PostCocApprovalCommandHandler> logger)
    : IRequestHandler<PostCocApprovalCommand, CocApprovalResult>
{
    public async Task<CocApprovalResult> Handle(PostCocApprovalCommand postCommand, CancellationToken cancellationToken)
    {
        logger.LogInformation("=== COMMITMENTS API: PostCocApprovalCommandHandler.Handle called ===");

        if (postCommand?.CocApprovalDetails == null)
        {
            throw new ArgumentNullException(nameof(postCommand));
        }

        var cocApprovalDetails = postCommand.CocApprovalDetails;

        var db = dbContext.Value;
        var existingApprovalRequests = db.ApprovalRequests.Where(r => r.LearningKey == cocApprovalDetails.LearningKey && r.Status == CocApprovalResultStatus.Pending);

        if (existingApprovalRequests.Any())
        {
            throw new DomainException("LearningKey", "An approval request for this learning key already exists.");
        }

        var updateStatuses = cocApprovalService.DetermineCocUpdateStatuses(cocApprovalDetails.Updates, cocApprovalDetails.Apprenticeship);
        var approvalRequestStatus = DetermineApprovalRequestStatus(updateStatuses);
        IEnumerable<ApprovalFieldRequest> approvalFieldRequests = MapToApprovalFieldRequests(cocApprovalDetails, updateStatuses);

        var approvalRequest = new ApprovalRequest
        {
            LearningKey = cocApprovalDetails.LearningKey,
            ApprenticeshipId = cocApprovalDetails.ApprenticeshipId,
            LearningType = cocApprovalDetails.LearningType,
            UKPRN = cocApprovalDetails.ProviderId.ToString(),
            ULN = cocApprovalDetails.ULN,
            Status = approvalRequestStatus,
            Items = approvalFieldRequests.ToList()
        };

        db.ApprovalRequests.Add(approvalRequest);

        return new CocApprovalResult
        {
            Status = approvalRequestStatus,
            Items = updateStatuses
        }; ;
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