using System;
using Paketti.Contexts;
using Paketti.Logging;
using Paketti.Utilities;

namespace Paketti.Library
{
    /// <summary>
    /// Builds a library of packages from a ProjectContext.
    /// </summary>
    public interface IProjectToLibraryBuilder
    {
        /// <summary>
        /// Builds the package library.
        /// </summary>
        /// <returns></returns>
        Result<ILibrary> Build(ProjectContext projectContext);
    }
}