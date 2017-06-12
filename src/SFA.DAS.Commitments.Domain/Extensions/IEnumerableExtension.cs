﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Commitments.Domain.Extensions
{
    public static class EnumerableExtension
    {
        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> items, Func<T, TKey> property)
        {
            return items.GroupBy(property).Select(x => x.First());
        }

        public static string ToListString<T>(this IEnumerable<T> items)
        {
            return items == null 
                ? string.Empty 
                : string.Join(",", items);
        }
    }
}
