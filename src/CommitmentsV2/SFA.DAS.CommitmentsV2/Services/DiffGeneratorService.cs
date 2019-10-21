using System;
using System.Collections.Generic;
using System.Reflection;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Mementos;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class DiffGeneratorService : IDiffGeneratorService
    {
        public IReadOnlyList<DiffItem> GenerateDiff(object initial, object updated)
        {
            if (initial != null && updated!= null && initial.GetType() != updated.GetType())
            {
                throw new ArgumentException("Diff generation can only be performed on objects of the same type");
            }

            var targetType = initial == null ? updated.GetType() : initial.GetType();

            var result = new List<DiffItem>();

            foreach (var property in targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var initialValue = initial == null ? null : property.GetValue(initial);
                var updatedValue = updated == null ? null : property.GetValue(updated);

                if (initialValue == null)
                {
                    if (updatedValue != null)
                    {
                        result.Add(new DiffItem
                        {
                            PropertyName = property.Name,
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
                        PropertyName = property.Name,
                        InitialValue = initialValue,
                        UpdatedValue = null
                    });
                    continue;
                }

                if (!initialValue.Equals(updatedValue))
                {
                    result.Add(new DiffItem
                    {
                        PropertyName = property.Name,
                        InitialValue = initialValue,
                        UpdatedValue = updatedValue
                    });
                }
            }

            return result;
        }
    }
}
