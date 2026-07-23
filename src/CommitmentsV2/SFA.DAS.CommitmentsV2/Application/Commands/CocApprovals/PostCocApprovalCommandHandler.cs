using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeship;

public class PostCocApprovalCommandHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ICocApprovalRulesEngine cocApprovalRules,
    ILogger<PostCocApprovalCommandHandler> logger,
    INotifyProviderService notifyProviderService)
    : IRequestHandler<PostCocApprovalCommand, CocApprovalResult>
{
    private const string ProviderRequestRejectedNotificationEmailTemplate = "ProviderRequestRejectedNotification";

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

        if (approvalState.ApprovalResult.Items.Any(i => i.Status == CocApprovalItemStatus.AutoRejected))
        {
            var providerName = db.Providers.Where(p => p.UkPrn == cocApprovalDetails.ProviderId).Select(p => p.Name).FirstOrDefault();

            await notifyProviderService.NotifyProvider(cocApprovalDetails.ProviderId, cocApprovalDetails.ApprenticeshipId, providerName, ProviderRequestRejectedNotificationEmailTemplate);
        }

        db.ApprovalRequests.Add(approvalState.ApprovalRequest);

        return approvalState.ApprovalResult;
    }
}