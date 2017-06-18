using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Paketti.Contexts;
using Paketti.Extensions;
using Paketti.Utilities;

namespace Paketti.Rewriters
{
    /// <summary>
    /// Collects and removes the (local type dependent) extension methods from a project.
    /// </summary>
    internal class ExtractExtensionMethodsRewriter
    {
        /// <summary>
        /// Gets the Type Members that were extracted.
        /// </summary>
        public IEnumerable<ITypeMemberContext> ExtractedMembers { get; private set; }

        /// <summary>
        /// Rewrites the ProjectContext within the specified DependencyWalker.
        /// </summary>
        /// <param name="dependencyWalker">The dependency walker.</param>
        /// <returns></returns>
        public Result<Project> Rewrite(IDependencyWalker dependencyWalker)
        {
            ExtractedMembers =
                dependencyWalker
                .ProjectContext
                .GetExtensionMethods()
                .Where(x => dependencyWalker.GetTypeDependencies(x).OnlyLocal().Any());

            //same code
            return ExtractTypeDependenciesRewriter.RemoveObjectsFromProject(dependencyWalker, ExtractedMembers);
        }
    }
}