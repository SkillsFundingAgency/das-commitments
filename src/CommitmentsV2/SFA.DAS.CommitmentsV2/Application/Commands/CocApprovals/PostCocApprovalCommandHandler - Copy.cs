using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeship;

public class PostCocApprovalCommandHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ICocApprovalRules cocApprovalRules,
    ILogger<PostCocApprovalCommandHandler> logger)
    : IRequestHandler<PostCocApprovalCommand, CocApprovalResult>
{
    public async Task<CocApprovalResult> Handle(PostCocApprovalCommand postCommand, CancellationToken cancellationToken)
    {
        logger.LogInformation("PostCocApprovalCommandHandler.Handle called");

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

        var approvalState = cocApprovalRules.DetermineApprovalState(cocApprovalDetails);

        db.ApprovalRequests.Add(approvalState.ApprovalRequest);

        return approvalState.ApprovalResult;
    }
}