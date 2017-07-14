using System;
using System.Collections.Generic;
using System.Linq;
using Paketti.Contexts;

namespace Paketti.Extensions
{
    internal static class TypeContextExtensions
    {
        /// <summary>
        /// Gets a value indicating whether any of the types are an interweave.
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public static bool AnyInterweaves(this IEnumerable<TypeContext> types)
            => types.Any(x => x.IsInterweave);

        /// <summary>
        /// Gets only the TypeContexts that are interweaves.
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public static IEnumerable<TypeContext> OnlyInterweaves(this IEnumerable<TypeContext> types)
            => types.Where(x => x.IsInterweave);
    }
}