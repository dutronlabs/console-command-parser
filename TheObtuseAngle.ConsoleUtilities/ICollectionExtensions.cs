using System.Collections.Generic;

namespace TheObtuseAngle.ConsoleUtilities
{
    internal static class ICollectionExtensions
    {
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> values)
        {
            foreach (var value in values)
            {
                collection.Add(value);
            }
        }

        public static void RemoveRange<T>(this ICollection<T> collection, IEnumerable<T> values)
        {
            foreach (var value in values)
            {
                collection.Remove(value);
            }
        }
    }
}
