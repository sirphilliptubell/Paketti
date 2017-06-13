using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
    internal static class PakettiIEnumerableExtensions
    {
        internal static IEnumerable<T> OnlyValues<T>(this IEnumerable<T> items)
            where T : class
            => items.Where(x => x != null);
    }
}