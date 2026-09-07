using System.Globalization;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Extensions;

namespace SFA.DAS.CommitmentsV2.Services;

public static class LearningChangeHistoryDescriptionBuilder
{
    private static readonly string[] PriceFields = ["TNP1", "TNP2"];

    public static string Build(IEnumerable<ApprovalFieldRequest> items)
    {
        if (items == null)
        {
            return string.Empty;
        }

        var autoRejected = items
            .Where(item => item.Status == CocApprovalItemStatus.AutoRejected)
            .ToList();

        if (autoRejected.Count == 0)
        {
            return string.Empty;
        }

        var descriptions = new List<string>();
        var priceFields = autoRejected
            .Where(item => PriceFields.Contains(item.Field, StringComparer.OrdinalIgnoreCase))
            .ToList();
        var otherFields = autoRejected
            .Where(item => !PriceFields.Contains(item.Field, StringComparer.OrdinalIgnoreCase));

        if (priceFields.Count > 0)
        {
            var oldTotal = SumAmounts(priceFields, item => item.Old);
            var newTotal = SumAmounts(priceFields, item => item.New);
            descriptions.Add($"Total price change from {oldTotal.ToGdsCostFormat()} to {newTotal.ToGdsCostFormat()}");
        }

        descriptions.AddRange(otherFields.Select(item =>
            $"{ToFieldDisplayName(item.Field)} change from {item.Old} to {item.New}"));

        return string.Join(". ", descriptions);
    }

    private static decimal SumAmounts(IEnumerable<ApprovalFieldRequest> fields, Func<ApprovalFieldRequest, string> selector)
    {
        return fields.Sum(field => ParseAmount(selector(field)));
    }

    private static decimal ParseAmount(string value)
    {
        return decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount)
            ? amount
            : 0;
    }

    private static string ToFieldDisplayName(string field)
    {
        return field switch
        {
            "DateOfBirth" => "Date of birth",
            _ => field
        };
    }
}
