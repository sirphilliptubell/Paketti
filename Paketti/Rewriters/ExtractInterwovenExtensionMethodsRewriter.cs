using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Paketti.Contexts;
using Paketti.Utilities;
using System.Linq;

namespace Paketti.Rewriters
{
    /// <summary>
    /// Collects and removes the interwoven extension methods from a project.
    /// </summary>
    internal class ExtractInterwovenExtensionMethodsRewriter
    {
        private readonly IPackageContentSelector _contentSelector;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractInterwovenExtensionMethodsRewriter"/> class.
        /// </summary>
        /// <param name="contentSelector">The content selector.</param>
        /// <exception cref="ArgumentException">contentSelector</exception>
        public ExtractInterwovenExtensionMethodsRewriter(IPackageContentSelector contentSelector)
        {
            _contentSelector = contentSelector ?? throw new ArgumentException(nameof(contentSelector));
        }

        /// <summary>
        /// Gets the Type Members that were extracted.
        /// </summary>
        public IEnumerable<MethodContext> ExtractedMembers { get; private set; }

        /// <summary>
        /// Extracts extension methods and also removes them from the project.
        /// </summary>
        /// <param name="dependencyWalker">The dependency walker.</param>
        /// <returns></returns>
        public Result<Project> Rewrite(IDependencyWalker dependencyWalker)
        {
            return _contentSelector
                .GetInterwovenExtensionMethods(dependencyWalker)
                //store what's being extracted
                .OnSuccessTee(extracted => ExtractedMembers = extracted)
                //remove extracted items from the project
                .OnSuccess(extracted => ExtractInterwovenTypeMembersRewriter.RemoveObjectsFromProject(dependencyWalker, extracted));
        }
    }
}