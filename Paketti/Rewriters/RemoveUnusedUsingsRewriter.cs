using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Paketti.Contexts;
using Paketti.Extensions;
using Paketti.Logging;

namespace Paketti.Rewriters
{
    /// <summary>
    /// Removes the unused using statements from the document.
    /// </summary>
    /// <seealso cref="Paketti.Rewriters.IRewriter" />
    internal class RemoveUnusedUsingsRewriter :
        IRewriter
    {
        /// <summary>
        /// Rewrites the specified document context.
        /// </summary>
        /// <param name="documentContext">The document context.</param>
        /// <param name="log">The log.</param>
        /// <returns></returns>
        public Result<Document> Rewrite(DocumentContext documentContext, ILog log)
        {
            using (log.LogStep($"{nameof(RemoveUnusedUsingsRewriter)}({documentContext.Name})"))
            {
                var state = new DocumentState(documentContext)
                    .AlterDocumentRoot(root =>
                    {
                        var usings = root.GetUnusedUsingDirectives();

                        return root.RemoveNodes(usings, SyntaxRemoveOptions.KeepNoTrivia);
                    });

                return state.Document;
            }
        }
    }
}