using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TheObtuseAngle.ConsoleUtilities
{
    internal static class IEnumerableExtensions
    {
        public static bool IsEmpty<T>(this IEnumerable<T> items)
        {
            return items == null || !items.Any();
        }

        public static bool HasItems<T>(this IEnumerable<T> items)
        {
            return items != null && items.Any();
        }

        public static bool HasDuplicates<TItem, TProp>(this IEnumerable<TItem> items, Func<TItem, TProp> selector)
        {
            int count = 0;
            var uniqueItems = new HashSet<TProp>();

            foreach (var item in items.Select(selector))
            {
                count++;
                uniqueItems.Add(item);
            }

            return count != uniqueItems.Count;
        }
    }
}
