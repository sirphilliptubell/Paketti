using System;
using System.Collections.Generic;
using System.Linq;
using Paketti.Contexts;

namespace Paketti.Extensions
{
    internal static class IKeyableExtensions
    {
        /// <summary>
        /// Gets a single key defined by a collection of keys.
        /// The keys are sorted to ensure two enumerations with the same keys will match.
        /// </summary>
        /// <param name="keyedItems"></param>
        /// <returns></returns>
        internal static string GetCollectiveOrderedKey(this IEnumerable<IKeyable> keyedItems)
            => keyedItems
            .Select(x => x.Key)
            .OrderBy(x => x)
            .ToCommaSeparated();
    }
}