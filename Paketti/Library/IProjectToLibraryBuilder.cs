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
        /// <param name="walker">The dependency walker for the project.</param>
        /// <param name="library">The libary to add to.</param>
        /// <param name="log">The logger.</param>
        /// <returns></returns>
        Result<ILibrary> Build(IDependencyWalker walker, ILibrary library, ILog log);
    }
}