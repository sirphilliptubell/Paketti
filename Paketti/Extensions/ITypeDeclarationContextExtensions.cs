using System.Collections.Generic;
using System.Linq;
using Paketti.Contexts;

namespace Paketti.Extensions
{
    public static class ITypeDeclarationContextExtensions
    {
        /// <summary>
        /// Gets the field/method/constructor/property contexts for this class/struct.
        /// Does not include extension methods.
        /// </summary>
        /// <param name="type">The class/struct/interface context.</param>
        /// <returns></returns>
        internal static IEnumerable<ITypeMemberContext> GetTypeMembersExcludingExtensions(this ITypeDeclarationContext type)
            => ((IEnumerable<ITypeMemberContext>)type.Fields)
            .Union(type.Methods.Where(x => !x.IsExtensionMethod))
            .Union(type.Constructors)
            .Union(type.Properties);
    }
}