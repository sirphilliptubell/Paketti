using System;
using Microsoft.CodeAnalysis;
using Paketti.Contexts;
using Paketti.Logging;

namespace Paketti.Rewriters
{
    public interface IRewriter
    {
        /// <summary>
        /// Rewrites the specified document context.
        /// </summary>
        /// <param name="documentContext">The document context.</param>
        /// <param name="log">The log.</param>
        /// <returns></returns>
        Result<Document> Rewrite(DocumentContext documentContext, ILog log);
    }
}