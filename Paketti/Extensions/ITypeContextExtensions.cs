using System.Collections.Generic;
using System.Linq;
using Paketti.Contexts;

namespace Paketti.Extensions
{
    public static class ITypeContextExtensions
    {
        /// <summary>
        /// Gets only the type dependencies which are in the local project.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public static IEnumerable<TypeContext> OnlyLocal(this IEnumerable<TypeContext> items)
            => items
            .Where(x => !x.IsCLRType && !x.IsGenericParameter);
    }
}