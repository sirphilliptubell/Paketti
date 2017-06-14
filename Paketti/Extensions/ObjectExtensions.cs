using System;

namespace Paketti.Extensions
{
    internal static class ObjectExtensions
    {
        internal static T Alter<T>(this T item, Func<T, T> alter)
            => alter(item);

        internal static U Map<T, U>(this T item, Func<T, U> map)
                    => map(item);
    }
}