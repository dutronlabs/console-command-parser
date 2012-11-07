using System;
using System.Collections.Generic;
using System.Linq;

namespace TheObtuseAngle.ConsoleUtilities
{
    internal static class IEnumerableExtensions
    {
        private static readonly Type stringType = typeof(string);

        public static bool IsEmpty<T>(this IEnumerable<T> items)
        {
            return items == null || !items.Any();
        }

        public static bool HasItems<T>(this IEnumerable<T> items)
        {
            return items != null && items.Any();
        }

        public static bool HasDuplicates<TItem>(this IEnumerable<TItem> items, bool ignoreNullOrEmptyElements)
        {
            return items.HasDuplicates(ignoreNullOrEmptyElements, o => o);
        }

        public static bool HasDuplicates<TItem, TProp>(this IEnumerable<TItem> items, bool ignoreNullOrEmptyElements, Func<TItem, TProp> selector)
        {
            bool isString = typeof(TProp) == stringType;
            int count = 0;
            var uniqueItems = new HashSet<TProp>();

            foreach (var item in items.Select(selector))
            {
                if (ignoreNullOrEmptyElements && (object.Equals(item, null) || (isString && string.IsNullOrWhiteSpace((string)(object)item))))
                {
                    continue;
                }

                count++;
                uniqueItems.Add(item);
            }

            return count != uniqueItems.Count;
        }
    }
}
