using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Paketti.Contexts;
using Paketti.Extensions;
using Paketti.Utilities;

namespace Paketti.Rewriters
{
    /// <summary>
    /// Gets the content for packages.
    /// </summary>
    public class PackageContentSelector :
        IPackageContentSelector
    {
        /// <summary>
        /// Gets the extension methods that are interwoven with non CLR types.
        /// </summary>
        /// <param name="dependencyWalker"></param>
        /// <returns></returns>
        public Result<IEnumerable<MethodContext>> GetInterwovenExtensionMethods(IDependencyWalker dependencyWalker)
        {
            var result = dependencyWalker
                .ProjectContext
                .GetExtensionMethods()
                .Where(x => dependencyWalker.GetTypeDependencies(x).AnyInterweaves());

            return Result.Ok(result);
        }

        /// <summary>
        /// Gets the instance and static type members that are interwoven with non CLR types.
        /// Excludes extension methods.
        /// </summary>
        /// <param name="dependencyWalker"></param>
        /// <returns></returns>
        public Result<IEnumerable<ITypeMemberContext>> GetInterwovenTypeMembersExceptExtensions(IDependencyWalker dependencyWalker)
        {
            var result =
                dependencyWalker
                .ProjectContext
                .GetTypeMembersExcludingExtensions()
                .Select(x => new
                {
                    Member = x,
                    Dependencies = dependencyWalker.GetTypeDependencies(x).Where(d => d.IsInterweave && d.FullNameWithoutGenericNames != x.ContainingTypeContext.Value.FullNameWithoutGenericNames)
                })
                .Where(x => x.Dependencies.Any())
                .Select(x => x.Member)
                .ToList();

            //Because we're getting only type members, eg: methods/delegates/etc.. of a struct/class/interface
            //then we can be sure that we've only retrieved items where the containing type is available.
            var missingContainingDeclaration =
                result
                .Where(x => x.ContainingTypeContext.HasNoValue);

            if (missingContainingDeclaration.Any())
                return Result.Fail<IEnumerable<ITypeMemberContext>>("Some type members are missing a containing type: " + missingContainingDeclaration.Select(x => x.Declaration.ToString()));

            return result;
        }

        /// <summary>
        /// Gets the member containers for the DependencyWalker
        /// </summary>
        /// <param name="dependencyWalker"></param>
        /// <returns></returns>
        public Result<(IEnumerable<ExtractedClassInfo> classes, IEnumerable<ExtractedStructInfo> structs)> GetMemberContainers(IDependencyWalker dependencyWalker)
        {
            var classes = dependencyWalker
                .ProjectContext
                .Documents
                .SelectMany(d => d.Classes.Select(c => new ExtractedClassInfo(d, c)));

            var structs = dependencyWalker
                .ProjectContext
                .Documents
                .SelectMany(d => d.Structs.Select(s => new ExtractedStructInfo(d, s)));

            return (classes, structs);
        }
    }
}