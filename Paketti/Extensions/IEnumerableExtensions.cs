using System;
using System.Collections.Generic;
using System.Linq;

namespace Paketti.Extensions
{
    internal static class PakettiIEnumerableExtensions
    {
        /// <summary>
        /// Filters the collection to only entries that are not null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        internal static IEnumerable<T> OnlyValues<T>(this IEnumerable<T> items)
            where T : class
            => items.Where(x => x != null);

        /// <summary>
        /// Gets the first item in the collection. Creates on if there are no values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The items.</param>
        /// <param name="create">The function that creates a value if the collection is empty.</param>
        /// <returns></returns>
        internal static T FirstOrCreate<T>(this IEnumerable<T> items, Func<T> create)
            => items
            //not using SingleOrDefault, as there may be a single item that is null and we
            //need to know the difference
            .Take(1)
            .ToList()
            .Map(list
                => list.Count == 1
                ? list[0]
                : create()
            );
    }
}