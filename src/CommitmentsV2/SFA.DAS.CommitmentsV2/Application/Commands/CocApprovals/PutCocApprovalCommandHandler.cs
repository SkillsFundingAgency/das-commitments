using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeship;

public class PutCocApprovalCommandHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ICocApprovalRules cocApprovalRules,
    ILogger<PostCocApprovalCommandHandler> logger)
    : IRequestHandler<PutCocApprovalCommand, CocApprovalResult>
{
    public async Task<CocApprovalResult> Handle(PutCocApprovalCommand putCommand, CancellationToken cancellationToken)
    {
        logger.LogInformation("PutCocApprovalCommandHandler.Handle called");

        if (putCommand?.CocApprovalDetails == null)
        {
            throw new ArgumentNullException(nameof(putCommand));
        }

        var cocApprovalDetails = putCommand.CocApprovalDetails;

        var db = dbContext.Value;
        var existingApprovalRequests = db.ApprovalRequests.Where(r => r.LearningKey == cocApprovalDetails.LearningKey && r.Status == CocApprovalResultStatus.Pending);

        if (!existingApprovalRequests.Any())
        {
            throw new DomainException("LearningKey", "A pending approval request for this learning key is expected but nothing exists.");
        }
        
        db.ApprovalRequests.RemoveRange(existingApprovalRequests);

        var approvalState = cocApprovalRules.DetermineApprovalState(cocApprovalDetails);

        db.ApprovalRequests.Add(approvalState.ApprovalRequest);

        return approvalState.ApprovalResult;
    }
}