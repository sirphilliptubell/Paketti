using System;

namespace Paketti.Extensions
{
    internal static class ObjectExtensions
    {
        /// <summary>
        /// Maps the specified value to a new type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="item">The item.</param>
        /// <param name="map">The mapping function.</param>
        /// <returns></returns>
        internal static U Map<T, U>(this T item, Func<T, U> map)
            => map(item);
    }
}