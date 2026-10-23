using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;

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
            logger.LogInformation("Cancelling Previous CocApprovalItems for LearningKey {0}", command.LearningKey);
            await MarkPendingItemsForLearningKeyAs(db, command.LearningKey, CocApprovalItemStatus.Cancelled);

            return new CocApprovalResult
            {
                Status = CocApprovalResultStatus.Cancelled,
                Items = new List<CocUpdateResult>()
            };
        }

        if (command.Action == AggregrationAction.SupersedePrevious)
        {
            logger.LogInformation("Superseding Previous CocApprovalItems for LearningKey {0}", command.LearningKey);
            await MarkPendingItemsForLearningKeyAs(db, command.LearningKey, CocApprovalItemStatus.Superseded);
        }

        var approvalState = await cocApprovalRules.DetermineApprovalState(command.CocApprovalDetails);

        if (approvalState.ApprovalRequest?.Items != null)
        {
            foreach (var approvalFieldRequest in approvalState.ApprovalRequest.Items)
            {
                db.CocApprovalItems.Add(new CocApprovalItem
                {
                    LearningKey = command.LearningKey,
                    ApprenticeshipId = approvalState.ApprovalRequest.ApprenticeshipId,
                    LearningType = approvalState.ApprovalRequest.LearningType,
                    UKPRN = approvalState.ApprovalRequest.UKPRN,
                    ULN = approvalState.ApprovalRequest.ULN,
                    Field = approvalFieldRequest.Field,
                    Old = approvalFieldRequest.Old,
                    New = approvalFieldRequest.New,
                    Status = approvalFieldRequest.Status
                });

                var fieldNameDescription = GetFieldDescription(approvalFieldRequest.Field) ?? approvalFieldRequest.Field;
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
        nameof(CocChangeField.Lastname) => "Last name",
        nameof(CocChangeField.PlannedEndDate) => "Planned end date",
        _ => null
    };

    private static async Task MarkPendingItemsForLearningKeyAs(ProviderCommitmentsDbContext db, Guid learningKey, CocApprovalItemStatus status)
    {
        var updated = DateTime.UtcNow;
        var items = await db.CocApprovalItems.Where(x => x.LearningKey == learningKey && x.Status == CocApprovalItemStatus.Pending).ToListAsync();

        foreach (var item in items)
        {
            item.Status = status;
            item.Updated = updated;
        }
    }
}