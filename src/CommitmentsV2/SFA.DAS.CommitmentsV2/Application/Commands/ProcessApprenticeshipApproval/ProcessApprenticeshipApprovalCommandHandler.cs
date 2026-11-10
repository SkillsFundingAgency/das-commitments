using System.Globalization;
using NServiceBus;
using Newtonsoft.Json;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Exceptions;
using SFA.DAS.CommitmentsV2.Mapping.CocApprovals;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;
using UserInfo = SFA.DAS.CommitmentsV2.Types.UserInfo;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ProcessApprenticeshipApproval;

public class ProcessApprenticeshipApprovalCommandHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
     IMessageSession messageSession, INotifyProviderService notifyProviderService,
     IEncodingService encodingService)
    : IRequestHandler<ProcessApprenticeshipApprovalCommand>
{
    public async Task Handle(ProcessApprenticeshipApprovalCommand command, CancellationToken cancellationToken)
    {
        var db = dbContext.Value;

        var pendingItems = await db.CocApprovalItems
            .Where(item => item.ApprenticeshipId == command.ApprenticeshipId && item.Status == CocApprovalItemStatus.Pending)
            .ToListAsync(cancellationToken);

        if (pendingItems.Count == 0)
        {
            throw new NotFoundException($"No pending changes were found for apprenticeship {command.ApprenticeshipId}");
        }

        var decision = command.ApplyChanges ? CocApprovalItemStatus.EmployerApproved : CocApprovalItemStatus.EmployerRejected;
        foreach (var item in pendingItems)
        {
            item.Status = decision;
            item.Updated = DateTime.UtcNow;
        }

        var priceItem = pendingItems.FirstOrDefault(item => item.Field == nameof(CocChangeField.Prices));
        var changes = ConvertItemsToChangeList(pendingItems, decision);
        var prices = ConvertPrices(priceItem?.New);
        if (command.ApplyChanges)
        {
            await messageSession.Publish(new LearningChangeApprovedEvent
            {
                LearningKey = pendingItems[0].LearningKey,
                ApprenticeshipId = command.ApprenticeshipId,
                Changes = changes,
                Prices = prices
            });

            await NotifyProvider(db, command.ApprenticeshipId, "ProviderLearningChangeApprovedNotification", cancellationToken);
        }
        else
        {
            await messageSession.Publish(new LearningChangeRejectedEvent
            {
                LearningKey = pendingItems[0].LearningKey,
                ApprenticeshipId = command.ApprenticeshipId,
                Changes = changes,
                Prices = prices,
                Reason = string.Empty
            });

            await NotifyProvider(db, command.ApprenticeshipId, "ProviderLearningChangeRejectedNotification", cancellationToken);
        }

        await RecordCocUpdatesInLearnerHistory(command.ApprenticeshipId, pendingItems, command.UserInfo, command.ApplyChanges);
    }

    private async Task NotifyProvider(ProviderCommitmentsDbContext db, long apprenticeshipId, string template, CancellationToken cancellationToken)
    {
        var details = await db.Apprenticeships.Where(x => x.Id == apprenticeshipId)
            .Include(x => x.Cohort)
            .Select(x => new { x.Cohort.ProviderId, x.Cohort.AccountLegalEntity.Name })
            .FirstOrDefaultAsync(cancellationToken);

        if (details == null)
        {
            throw new NotFoundException($"Apprenticeship {apprenticeshipId} not found");
        }

        if (details.ProviderId <= 0 || details.Name is null)
        {
            throw new NotFoundException($"Cohort details for apprenticeship {apprenticeshipId} not found");
        }

        var apprencticeshipIdEncoded = encodingService.Encode(apprenticeshipId, EncodingType.ApprenticeshipId);

        await notifyProviderService.NotifyProvider(details.ProviderId, apprencticeshipIdEncoded, template, details.Name);
    }

    private async Task RecordCocUpdatesInLearnerHistory(long apprenticeshipId, ICollection<CocApprovalItem> items, UserInfo userInfo, bool applyChanges)
    {
        foreach (var item in items)
        {
            var description = item.Field == nameof(CocChangeField.Prices)
                ? CreatePriceHistoryDescription(item)
                : $"{GetFieldDescription(item.Field)} change from {item.Old} to {item.New}";

            await messageSession.Send(new StoreLearningHistoryCommand
            {
                ApprenticeshipId = apprenticeshipId,
                Source = LearningSourceType.ApprovalAPI,
                ChangeType = applyChanges ? LearningChangeType.EmployerApproved : LearningChangeType.EmployerRejected,
                AppliedDate = DateTime.UtcNow,
                Description = description,
                UserId = GetUserId(userInfo)
            });
        }
    }

    private static string CreatePriceHistoryDescription(CocApprovalItem item)
    {
        var totalOldValue = PriceScheduleMapper.GetLatestCost(item.Old) ?? 0;
        var totalNewValue = PriceScheduleMapper.GetLatestCost(item.New) ?? 0;
        return $"Total price change from {ToCurrency(totalOldValue)} to {ToCurrency(totalNewValue)}";
    }

    private static string GetFieldDescription(string field)
    {
        return field switch
        {
            nameof(CocChangeField.PlannedEndDate) => "Planned end date",
            _ => field
        };
    }

    public static string ToCurrency(decimal input)
    {
        var culture = new CultureInfo("en-GB");
        return input.ToString("C0", culture);
    }

    private static Guid? GetUserId(UserInfo userInfo)
    {
        if (userInfo?.UserId != null && Guid.TryParse(userInfo.UserId, out var userId))
        {
            return userId;
        }

        return null;
    }

    private static List<LearningChangeEvent.Change> ConvertItemsToChangeList(
        ICollection<CocApprovalItem> items,
        CocApprovalItemStatus decision)
    {
        var changes = new List<LearningChangeEvent.Change>();
        foreach (var item in items.Where(item => item.Field != nameof(CocChangeField.Prices)))
        {
            changes.Add(new LearningChangeEvent.Change
            {
                ChangeType = item.Field,
                ApprovalStatus = decision == CocApprovalItemStatus.EmployerApproved
                    ? nameof(CocApprovalItemStatus.EmployerApproved)
                    : nameof(CocApprovalItemStatus.EmployerRejected),
                Data = new LearningChangeEvent.ChangeData
                {
                    Old = item.Old,
                    New = item.New
                }
            });
        }
        return changes;
    }

    private static List<LearningChangeEvent.Price> ConvertPrices(string scheduleJson)
    {
        if (string.IsNullOrWhiteSpace(scheduleJson))
        {
            return null;
        }

        return JsonConvert.DeserializeObject<List<LearningChangeEvent.Price>>(scheduleJson);
    }
}
