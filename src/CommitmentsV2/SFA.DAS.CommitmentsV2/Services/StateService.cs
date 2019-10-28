using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class StateService : IStateService
    {
        public Dictionary<string, object> GetState(object item)
        {
            var result = new Dictionary<string, object>();
            var targetType = item.GetType();

            foreach (var property in targetType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var propertyType = property.PropertyType;

                if (!propertyType.IsClass && propertyType.GetInterface(nameof(IEnumerable)) == null)
                {
                    result.Add(property.Name, property.GetValue(item));
                }
            }

            return result;
        }

        public IReadOnlyList<DiffItem> GenerateDiff(Dictionary<string, object> initial, Dictionary<string, object> updated)
        {
            var result = new List<DiffItem>();

            if (initial == null)
            {
                if(updated == null)
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
}
