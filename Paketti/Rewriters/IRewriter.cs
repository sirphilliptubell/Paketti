using System;
using Microsoft.CodeAnalysis;
using Paketti.Contexts;
using Paketti.Logging;

namespace Paketti.Rewriters
{
    public interface IRewriter
    {
        /// <summary>
        /// Gets a value indicating whether the project should be recompiled to validate the changes
        /// didn't break the project.
        /// </summary>
        /// <value>
        ///   <c>true</c> if you should recompile to validate the project; otherwise, <c>false</c>.
        /// </value>
        bool ShouldRecompileToValidate { get; }

        /// <summary>
        /// Rewrites the specified document context.
        /// </summary>
        /// <param name="documentContext">The document context.</param>
        /// <param name="log">The log.</param>
        /// <returns></returns>
        Result<Document> Rewrite(DocumentContext documentContext, ILog log);
    }
}