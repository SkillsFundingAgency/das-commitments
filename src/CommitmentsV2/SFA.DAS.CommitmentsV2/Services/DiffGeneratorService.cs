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
            if (initial.GetType() != updated.GetType())
            {
                throw new ArgumentException("Diff generation can only be performed on objects of the same type");
            }

            var result = new List<DiffItem>();

            var type = typeof(CohortMemento);
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var initialValue = property.GetValue(initial);
                var updatedValue = property.GetValue(updated);

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
