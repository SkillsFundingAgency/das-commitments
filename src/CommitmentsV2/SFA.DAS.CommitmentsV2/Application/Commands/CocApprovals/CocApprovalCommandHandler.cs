using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Mapping.CocApprovals;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;

public class CocApprovalCommandHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ICocApprovalRulesEngine cocApprovalRules,
    ILogger<CocApprovalCommandHandler> logger,
    IMessageSession messageSession,
    ICurrentDateTime currentDateTime,
    INotifyProviderService notifyProviderService,
    IEncodingService encodingService)
    : IRequestHandler<CocApprovalCommand, CocApprovalResult>
{
    private const string ProviderRequestRejectedNotificationEmailTemplate = "ProviderRequestRejectedNotification";

    private readonly PriceChangeDecider _priceChangeDecider = new(new PriceScheduleMapper());

    public async Task<CocApprovalResult> Handle(CocApprovalCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("CocApprovalCommandHandler.Handle called");

        if (command == null)
        {
            throw new ArgumentNullException(nameof(command));
        }

        var db = dbContext.Value;

        await MarkFields(db, command.LearningKey, command.ReversedFields, CocApprovalItemStatus.Reversal);

        if (command.Action == AggregationAction.CancelPrevious)
        {
            logger.LogInformation("Cancelling Previous CocApprovalItems for LearningKey {0}", command.LearningKey);
            var cancelledFields = command.CocApprovalDetails?.ApprovalFieldChanges?
                .Select(change => change.ChangeType)
                .ToList();
            await MarkFields(db, command.LearningKey, cancelledFields, CocApprovalItemStatus.Reversal);

            return new CocApprovalResult
            {
                Status = CocApprovalResultStatus.Cancelled,
                Items = new List<CocUpdateResult>()
            };
        }

        if (command.Action == AggregationAction.SupersedePrevious)
        {
            var replacedFields = command.CocApprovalDetails?.ApprovalFieldChanges?
                .Select(change => change.ChangeType)
                .Where(field => field != nameof(CocChangeField.Prices))
                .ToList();
            logger.LogInformation("Superseding Previous CocApprovalItems for LearningKey {0}", command.LearningKey);
            await MarkFields(db, command.LearningKey, replacedFields, CocApprovalItemStatus.Superseded);
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
                    Status = approvalFieldRequest.Status.Value,
                    Reason = approvalFieldRequest.Reason
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

        await ApplyPriceDecision(db, command, approvalState);

        return approvalState.ApprovalResult;
    }

    private async Task ApplyPriceDecision(ProviderCommitmentsDbContext db, CocApprovalCommand command, CocApprovalState approvalState)
    {
        var decision = _priceChangeDecider.Decide(
            command.Prices,
            command.CocApprovalDetails?.Apprenticeship?.PriceHistory,
            currentDateTime.UtcNow);

        if (decision == null)
        {
            return;
        }

        approvalState.ApprovalResult ??= new CocApprovalResult { Items = [] };
        approvalState.ApprovalResult.Items ??= [];

        var anotherFieldWasAutoRejected = approvalState.ApprovalResult.Items.Any(item => item.Status == CocApprovalItemStatus.AutoRejected);

        if (decision.PendingPriceAction == PendingPriceAction.MarkReversal)
        {
            await MarkFields(db, command.LearningKey, [nameof(CocChangeField.Prices)], CocApprovalItemStatus.Reversal);
        }
        else if (decision.PendingPriceAction == PendingPriceAction.MarkSuperseded)
        {
            await MarkFields(db, command.LearningKey, [nameof(CocChangeField.Prices)], CocApprovalItemStatus.Superseded);
        }

        if (decision.StoreNewRow)
        {
            var approvalRequest = approvalState.ApprovalRequest;
            db.CocApprovalItems.Add(new CocApprovalItem
            {
                LearningKey = command.LearningKey,
                ApprenticeshipId = approvalRequest?.ApprenticeshipId ?? command.CocApprovalDetails.ApprenticeshipId,
                LearningType = approvalRequest?.LearningType ?? command.CocApprovalDetails.LearningType,
                UKPRN = approvalRequest?.UKPRN ?? command.CocApprovalDetails.ProviderId.ToString(),
                ULN = approvalRequest?.ULN ?? command.CocApprovalDetails.ULN,
                Field = nameof(CocChangeField.Prices),
                Old = decision.Old,
                New = decision.New,
                Status = decision.Status,
                Reason = decision.Reason
            });
        }

        if (decision.Status == CocApprovalItemStatus.AutoRejected)
        {
            await CreateLearningHistoryAsync(command.CocApprovalDetails, LearningSourceType.ApprovalAPI, LearningChangeType.AutoRejected, decision.Reason);
            if (!anotherFieldWasAutoRejected)
            {
                var apprenticeship = encodingService.Encode(command.CocApprovalDetails.ApprenticeshipId, EncodingType.ApprenticeshipId);
                await notifyProviderService.NotifyProvider(command.CocApprovalDetails.ProviderId, apprenticeship, ProviderRequestRejectedNotificationEmailTemplate);
            }
        }

        approvalState.ApprovalResult.Items.Add(new CocUpdateResult
        {
            Field = CocChangeField.Prices,
            Status = decision.Status,
            Reason = decision.Reason
        });
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

    private static async Task MarkPendingItemsForLearningKeyAs(ProviderCommitmentsDbContext db, Guid learningKey, CocApprovalItemStatus status, IReadOnlyCollection<string> fields)
    {
        var updated = DateTime.UtcNow;
        var items = await db.CocApprovalItems.Where(x => x.LearningKey == learningKey && x.Status == CocApprovalItemStatus.Pending).ToListAsync();

        foreach (var item in items.Where(item => fields == null || fields.Contains(item.Field)))
        {
            item.Status = status;
            item.Updated = updated;
        }
    }

    private static Task MarkFields(ProviderCommitmentsDbContext db, Guid learningKey, IReadOnlyCollection<string> fields, CocApprovalItemStatus status)
    {
        if (fields == null || fields.Count == 0)
        {
            return Task.CompletedTask;
        }

        return MarkPendingItemsForLearningKeyAs(db, learningKey, status, fields);
    }
}
