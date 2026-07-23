using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Exceptions;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeship;

public class PutCocApprovalCommandHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ICocApprovalRulesEngine cocApprovalRules,
    ILogger<PostCocApprovalCommandHandler> logger,
    INotifyProviderService notifyProviderService)
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
        var existingApprovalRequests = await db.ApprovalRequests.Where(r => r.LearningKey == cocApprovalDetails.LearningKey && r.Status == CocApprovalResultStatus.Pending).ToListAsync(cancellationToken);

        if (!existingApprovalRequests.Any())
        {
            throw new PendingApprovalNotFoundException("There is no pending change to override.");
        }

        MarkAsSuperseded(db, existingApprovalRequests);

        var approvalState = cocApprovalRules.DetermineApprovalState(cocApprovalDetails);

        if (approvalState.ApprovalResult.Items.Any(i => i.Status == CocApprovalItemStatus.AutoRejected))
        {
            var providerName = db.Providers.Where(p => p.UkPrn == cocApprovalDetails.ProviderId).Select(p => p.Name).FirstOrDefault();

            await notifyProviderService.NotifyProvider(cocApprovalDetails.ProviderId, cocApprovalDetails.ApprenticeshipId, providerName, "ProviderRequestRejectedNotification");
        }

        db.ApprovalRequests.Add(approvalState.ApprovalRequest);

        return approvalState.ApprovalResult;
    }

    private static void MarkAsSuperseded(ProviderCommitmentsDbContext db, List<ApprovalRequest> existingApprovalRequests)
    {
        var updated = DateTime.UtcNow;
        existingApprovalRequests.ForEach(r =>
        {
            r.Status = CocApprovalResultStatus.Superseded;
            r.Updated = updated;
        });
        db.ApprovalRequests.UpdateRange(existingApprovalRequests);
    }
}