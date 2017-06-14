using System;
using System.Collections.Generic;
using System.Linq;

namespace Paketti.Extensions
{
    internal static class PakettiIEnumerableExtensions
    {
        internal static IEnumerable<T> OnlyValues<T>(this IEnumerable<T> items)
            where T : class
            => items.Where(x => x != null);

        internal static T SingleOrCreate<T>(this IEnumerable<T> items, Func<T> create)
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