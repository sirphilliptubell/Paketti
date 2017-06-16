using System;
using Paketti.Contexts;
using Paketti.Logging;

namespace Paketti.Utilities
{
    /// <summary>
    /// Represents a .net compiler.
    /// </summary>
    public interface ICompiler
    {
        /// <summary>
        /// Compiles the specified project.
        /// </summary>
        /// <param name="projectContext">The project context.</param>
        /// <param name="log">The log.</param>
        /// <returns></returns>
        Result Compile(ProjectContext projectContext, ILog log);
    }
}