using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Extensions;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeship;

public class CocApprovalCommandHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ICocApprovalRulesEngine cocApprovalRules,
    ILogger<CocApprovalCommandHandler> logger,
    IMessageSession messageSession,
    ICurrentDateTime currentDateTime)
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

        if (approvalState.ApprovalRequest?.Items != null)
        {
            if (approvalState.ApprovalRequest.Items.Any(x => (x.Field == nameof(CocChangeField.TNP1) || x.Field == nameof(CocChangeField.TNP2)) && x.Status == CocApprovalItemStatus.AutoRejected))
            {
                var oldTotal = (command.CocApprovalDetails.Updates?.TNP1?.Old ?? 0) + (command.CocApprovalDetails.Updates?.TNP2?.Old ?? 0);
                var newTotal = (command.CocApprovalDetails.Updates?.TNP1?.New ?? 0) + (command.CocApprovalDetails.Updates?.TNP2?.New ?? 0);

                await CreateLearningHistoryAsync(command.CocApprovalDetails, LearningSourceType.ApprovalAPI, LearningChangeType.AutoRejected, $"Total price change from {oldTotal.ToGdsCostFormat()} to {newTotal.ToGdsCostFormat()}");
            }
            foreach (var approvalFieldRequest in approvalState.ApprovalRequest.Items.Where(x => x.Field != nameof(CocChangeField.TNP1) && x.Field != nameof(CocChangeField.TNP2)))
            {
                var fieldNameDescription = GetFieldDescription(approvalFieldRequest.Field);
                if (approvalFieldRequest.Status == CocApprovalItemStatus.AutoRejected)
                {
                    await CreateLearningHistoryAsync(command.CocApprovalDetails, LearningSourceType.ApprovalAPI, LearningChangeType.AutoRejected, $"{fieldNameDescription} change from {approvalFieldRequest.Old} to {approvalFieldRequest.New}");
                }
                else if (approvalFieldRequest.Status == CocApprovalItemStatus.AutoApproved)
                {
                    await CreateLearningHistoryAsync(command.CocApprovalDetails, LearningSourceType.ApprovalAPI, LearningChangeType.AutoApproved, $"{fieldNameDescription} change from {approvalFieldRequest.Old} to {approvalFieldRequest.New}");
                }
            }
        }

        return approvalState.ApprovalResult;
    }

    private async Task CreateLearningHistoryAsync(CocApprovalDetails details, LearningSourceType learningSourceType, LearningChangeType learningChangeType, string description)
    {
        await messageSession.Send(new StoreLearningHistoryCommand
        {
            ApprenticeshipId = details.ApprenticeshipId,
            LearningKey = details.LearningKey,
            Source = learningSourceType,
            ChangeType = learningChangeType,
            AppliedDate = currentDateTime.UtcNow,
            Description = description
        });
    }

    private string GetFieldDescription(string cocChangeField) => cocChangeField switch
    {
        nameof(CocChangeField.Firstname) => "First name",
        _ => null
    };

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