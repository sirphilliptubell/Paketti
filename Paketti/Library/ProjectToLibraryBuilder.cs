using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Paketti.Contexts;
using Paketti.Extensions;
using Paketti.Logging;
using Paketti.Utilities;

namespace Paketti.Library
{
    /// <summary>
    /// Builds a library of packages from a ProjectContext.
    /// </summary>
    /// <seealso cref="Paketti.Packaging.IPackager" />
    public class ProjectToLibraryBuilder :
        IProjectToLibraryBuilder
    {
        /// <summary>
        /// Builds the package library.
        /// </summary>
        /// <returns></returns>
        public Result<ILibrary> Build(IDependencyWalker walker, ILibrary library, ILog log)
        {
            if (walker == null) throw new ArgumentException(nameof(walker));
            if (library == null) throw new ArgumentException(nameof(library));
            if (log == null) throw new ArgumentException(nameof(log));

            var projCtx = walker.ProjectContext;

            AddExtensionMethods(projCtx, walker, library, log);

            return Result.Ok(library);
        }

        /// <summary>
        /// Adds the extension methods of the project.
        /// </summary>
        private void AddExtensionMethods(ProjectContext projectContext, IDependencyWalker walker, ILibrary library, ILog log)
        {
            using (log.LogStep($"{nameof(ProjectToLibraryBuilder)}.{nameof(AddExtensionMethods)}"))
            {
                var groups =
                    projectContext
                    .GetExtensionMethods()
                    .GroupBy(x => walker.GetTypeDependencies(x).OnlyLocal())
                    .ToList();

                foreach (var group in groups)
                {
                    var content =
                        group
                        .Select(x => x.ToFormattedCode())
                        .ToSeparatedString(Config.PACKAGE_LINE_SEPARATOR);

                    var dependencies =
                        group
                        .Key
                        .Select(x => x.ToString());

                    library.AddOrMerge(new ExtensionMethodsPackage(content, dependencies));
                }
            }
        }
    }
}