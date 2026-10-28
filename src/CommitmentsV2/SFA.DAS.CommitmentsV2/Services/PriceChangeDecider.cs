using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Mapping.CocApprovals;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Services;

public class PriceChangeDecider(PriceScheduleMapper priceScheduleMapper)
{
    public const string FuturePriceReason = "The price change includes a date in the future.";

    public PriceChangeDecision Decide(IReadOnlyList<PriceRecord> prices, IEnumerable<PriceHistory> priceHistory, DateTime utcToday)
    {
        if (prices == null || prices.Count == 0)
        {
            return null;
        }

        var history = priceHistory ?? [];
        var oldJson = priceScheduleMapper.Map(history);
        var newJson = priceScheduleMapper.Map(prices);

        if (prices.Any(price => price.EffectiveFrom.Date > utcToday.Date))
        {
            return new PriceChangeDecision
            {
                Status = CocApprovalItemStatus.AutoRejected,
                Reason = FuturePriceReason,
                Old = oldJson,
                New = newJson,
                StoreNewRow = true,
                PendingPriceAction = PendingPriceAction.MarkSuperseded
            };
        }

        if (priceScheduleMapper.LatestEpisodesMatch(history, prices))
        {
            return new PriceChangeDecision
            {
                Status = CocApprovalItemStatus.Reversal,
                Old = oldJson,
                New = newJson,
                StoreNewRow = false,
                PendingPriceAction = PendingPriceAction.MarkReversal
            };
        }

        return new PriceChangeDecision
        {
            Status = CocApprovalItemStatus.Pending,
            Old = oldJson,
            New = newJson,
            StoreNewRow = true,
            PendingPriceAction = PendingPriceAction.MarkSuperseded
        };
    }
}

public class PriceChangeDecision
{
    public CocApprovalItemStatus Status { get; init; }
    public string Reason { get; init; }
    public string Old { get; init; }
    public string New { get; init; }
    public bool StoreNewRow { get; init; }
    public PendingPriceAction PendingPriceAction { get; init; }
}

public enum PendingPriceAction
{
    MarkReversal,
    MarkSuperseded
}
