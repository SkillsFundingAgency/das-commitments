using System.Globalization;
using NServiceBus;
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
    private readonly PriceScheduleMapper _priceScheduleMapper = new();

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

        var changes = ConvertItemsToChangeDictionary(pendingItems);
        if (command.ApplyChanges)
        {
            await messageSession.Publish(new LearningChangeApprovedEvent
            {
                LearningKey = pendingItems[0].LearningKey,
                ApprenticeshipId = command.ApprenticeshipId,
                Changes = changes
            });

            await NotifyProvider(db, command.ApprenticeshipId, "ProviderLearningChangeApprovedNotification", cancellationToken);
        }
        else
        {
            await messageSession.Publish(new LearningChangeRejectedEvent
            {
                LearningKey = pendingItems[0].LearningKey,
                ApprenticeshipId = command.ApprenticeshipId,
                Changes = changes
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
            throw new Exception($"Apprenticeship {apprenticeshipId} not found");
        }

        if (details.ProviderId <= 0 || details.Name is null)
        {
            throw new Exception($"Cohort details for apprenticeship {apprenticeshipId} not found");
        }

        var apprencticeshipIdEncoded = encodingService.Encode(apprenticeshipId, EncodingType.ApprenticeshipId);

        await notifyProviderService.NotifyProvider(details.ProviderId, apprencticeshipIdEncoded, template, details.Name);
    }

    private async Task RecordCocUpdatesInLearnerHistory(long apprenticeshipId, ICollection<CocApprovalItem> items, UserInfo userInfo, bool applyChanges)
    {
        var pricesItem = items.FirstOrDefault(item => item.Field == nameof(CocChangeField.Prices));
        if (pricesItem == null)
        {
            return;
        }

        var totalOldValue = _priceScheduleMapper.GetLatestCost(pricesItem.Old) ?? 0;
        var totalNewValue = _priceScheduleMapper.GetLatestCost(pricesItem.New) ?? 0;

        await messageSession.Send(new StoreLearningHistoryCommand
        {
            ApprenticeshipId = apprenticeshipId,
            Source = LearningSourceType.ApprovalAPI,
            ChangeType = applyChanges ? LearningChangeType.EmployerApproved : LearningChangeType.EmployerRejected,
            AppliedDate = DateTime.UtcNow,
            Description = $"Total price change from {ToCurrency(totalOldValue)} to {ToCurrency(totalNewValue)}",
            UserId = GetUserId(userInfo)
        });
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

    private static Dictionary<string, LearningChangeEvent.Change> ConvertItemsToChangeDictionary(ICollection<CocApprovalItem> items)
    {
        var changes = new Dictionary<string, LearningChangeEvent.Change>();
        foreach (var item in items)
        {
            changes[item.Field] = new LearningChangeEvent.Change
            {
                Old = item.Old,
                New = item.New,
                EffectiveFromDate = null
            };
        }
        return changes;
    }
}
