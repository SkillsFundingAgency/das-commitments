using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Services;

public class DiffService : IDiffService
{
    public IReadOnlyList<DiffItem> GenerateDiff(Dictionary<string, object> initial, Dictionary<string, object> updated)
    {
        var result = new List<DiffItem>();

        if (initial == null)
        {
            if (updated == null)
            {
                return new List<DiffItem>();
            }

            foreach (var item in updated.Where(x => x.Value != null))
            {
                result.Add(new DiffItem
                {
                    PropertyName = item.Key,
                    InitialValue = null,
                    UpdatedValue = item.Value
                });
            }

            return result;
        }

        foreach (var item in initial)
        {
            var initialValue = item.Value;
            var updatedValue = updated == null ? null : updated.ContainsKey(item.Key) ? updated[item.Key] : null;

            if (initialValue == null)
            {
                if (updatedValue != null)
                {
                    result.Add(new DiffItem
                    {
                        PropertyName = item.Key,
                        InitialValue = null,
                        UpdatedValue = updatedValue
                    });
                }
                continue;
            }

            if (updatedValue == null)
            {
                result.Add(new DiffItem
                {
                    PropertyName = item.Key,
                    InitialValue = initialValue,
                    UpdatedValue = null
                });
                continue;
            }

            if (!initialValue.Equals(updatedValue))
            {
                result.Add(new DiffItem
                {
                    PropertyName = item.Key,
                    InitialValue = initialValue,
                    UpdatedValue = updatedValue
                });
            }
        }

        return result;
    }
}