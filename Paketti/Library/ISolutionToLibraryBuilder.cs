using System;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.FileProviders;

namespace Paketti.Library
{
    /// <summary>
    /// Builds a package library from a solution.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISolutionToLibraryBuilder<T>
        where T : Workspace
    {
        /// <summary>
        /// Builds the package library.
        /// </summary>
        Result<ILibrary> Build();
    }
}