using System;
using System.Collections.Generic;
using Paketti.Contexts;
using Paketti.Utilities;

namespace Paketti.Rewriters
{
    /// <summary>
    /// Gets the content for packages.
    /// </summary>
    public interface IPackageContentSelector
    {
        /// <summary>
        /// Gets the extension methods that are interwoven with non CLR types.
        /// </summary>
        /// <param name="dependencyWalker"></param>
        /// <returns></returns>
        Result<IEnumerable<MethodContext>> GetInterwovenExtensionMethods(IDependencyWalker dependencyWalker);

        /// <summary>
        /// Gets the instance and static type members that are interwoven with non CLR types.
        /// Excludes extension methods.
        /// </summary>
        /// <param name="dependencyWalker"></param>
        /// <returns></returns>
        Result<IEnumerable<ITypeMemberContext>> GetInterwovenTypeMembersExceptExtensions(IDependencyWalker dependencyWalker);

        /// <summary>
        /// Gets the instance and static types that can contain members.
        /// </summary>
        /// <param name="dependencyWalker"></param>
        /// <param name="usings"></param>
        /// <returns></returns>
        Result<(IEnumerable<ExtractedClassInfo> classes, IEnumerable<ExtractedStructInfo> structs)> GetMemberContainers(IDependencyWalker dependencyWalker);
    }
}