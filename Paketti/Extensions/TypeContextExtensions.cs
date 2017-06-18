using System.Collections.Generic;
using System.Linq;
using Paketti.Contexts;

namespace Paketti.Extensions
{
    public static class TypeContextExtensions
    {
        /// <summary>
        /// Filters the types to only those that are not part of the CLR and are not generic parameters.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public static IEnumerable<TypeContext> OnlyLocal(this IEnumerable<TypeContext> items)
            => items
            .Where(x => !x.IsCLRType && !x.IsGenericParameter);
    }
}