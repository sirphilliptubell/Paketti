using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Paketti.Contexts;
using Paketti.Extensions;
using Paketti.Primitives;

namespace Paketti.Contexts
{
    /// <summary>
    /// Handles formatting of the names for types.
    /// </summary>
    internal static class NameBuilder
    {
        /// <summary>
        /// Gets the full name.
        /// </summary>
        /// <param name="typeArguments">The type arguments.</param>
        /// <param name="name">The name.</param>
        /// <param name="isGeneric">if set to <c>true</c> [is generic].</param>
        /// <param name="isValueTuple">if set to <c>true</c> [is value tuple].</param>
        /// <param name="isArray">if set to <c>true</c> [is array].</param>
        /// <param name="includeGenericTypeParamNames">if set to <c>true</c> [include generic type parameter names].</param>
        /// <returns></returns>
        internal static string GetFullName(
            IEnumerable<TypeContext> typeArguments,
            string name,
            bool isGeneric,
            bool isValueTuple,
            bool isArray,
            bool includeGenericTypeParamNames)
        {
            var argCountLess1 = typeArguments.Count() - 1;
            if (argCountLess1 == -1)
                argCountLess1 = 0;

            var typeArgs
                = includeGenericTypeParamNames
                ? typeArguments.Select(x => x.Name).ToCommaSeparated() //comma separate the names
                : string.Empty.PadRight(argCountLess1, ','); //only use commas, eg: Tuple<,,,>

            var generic
                = isGeneric
                ? $"<{typeArgs}>"
                : string.Empty;

            var vTuple
                = isValueTuple
                ? $"ValueTuple<{typeArgs}>"

                : string.Empty;


            var array
                = isArray
                ? $"{typeArguments.Single().Name}[]"
                : string.Empty;

            /*
             * do not include the assembly name.
             * */

            return $"{name}{generic}{vTuple}{array}";
        }
    }
}