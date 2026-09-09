using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Exceptions;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeship;

public class CocApprovalCommandHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ICocApprovalRulesEngine cocApprovalRules,
    ILogger<CocApprovalCommandHandler> logger)
    : IRequestHandler<CocApprovalCommand, CocApprovalResult>
{
    public async Task<CocApprovalResult> Handle(CocApprovalCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("CocApprovalCommandHandler.Handle called");

        if (command == null)
        {
            throw new ArgumentNullException(nameof(command));
        }

        var db = dbContext.Value;

        if (command.Action == AggregrationAction.CancelPrevious)
        {
            logger.LogInformation("Cancelling Previous ApprovalRequest {0}", command.PreviousApprovalRequestId);
            if (!command.PreviousApprovalRequestId.HasValue)
            {
                throw new ArgumentNullException(nameof(command.PreviousApprovalRequestId));
            }
            var existingApprovalRequest = await db.ApprovalRequests.FindAsync(command.PreviousApprovalRequestId.Value);
            if(existingApprovalRequest == null)
            {
                throw new KeyNotFoundException($"Could not find PreviousApprovalRequestId {command.PreviousApprovalRequestId}");
            }
            MarkAsCancelled(db, existingApprovalRequest);

            return new CocApprovalResult
            {
                Status = CocApprovalResultStatus.Cancelled,
                Items = new List<CocUpdateResult>()
            };
        }

        if (command.Action == AggregrationAction.SupersedePrevious)
        {
            logger.LogInformation("Superseding Previous ApprovalRequest {0}", command.PreviousApprovalRequestId);
            if (!command.PreviousApprovalRequestId.HasValue)
            {
                throw new ArgumentNullException(nameof(command.PreviousApprovalRequestId));
            }
            var existingRequest = await db.ApprovalRequests.FindAsync(command.PreviousApprovalRequestId.Value);
            MarkAsSuperseded(db, existingRequest);
        }

        var approvalState = await cocApprovalRules.DetermineApprovalState(command.CocApprovalDetails);

        db.ApprovalRequests.Add(approvalState.ApprovalRequest);

        return approvalState.ApprovalResult;
    }

    private static void MarkAsSuperseded(ProviderCommitmentsDbContext db, ApprovalRequest existingApprovalRequest)
    {
        var updated = DateTime.UtcNow;
        existingApprovalRequest.Status = CocApprovalResultStatus.Superseded;
        existingApprovalRequest.Updated = updated;
        
        db.ApprovalRequests.Update(existingApprovalRequest);
    }

    private static void MarkAsCancelled(ProviderCommitmentsDbContext db, ApprovalRequest existingApprovalRequest)
    {
        var updated = DateTime.UtcNow;
        existingApprovalRequest.Status = CocApprovalResultStatus.Cancelled;
        existingApprovalRequest.Updated = updated;

        db.ApprovalRequests.Update(existingApprovalRequest);
    }

}