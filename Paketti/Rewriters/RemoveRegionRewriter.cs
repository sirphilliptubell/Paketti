using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Paketti.Contexts;
using Paketti.Logging;

namespace Paketti.Rewriters
{
    /// <summary>
    /// Removes all #region and #endregion directives from a document.
    /// </summary>
    /// <seealso cref="Paketti.Rewriters.IRewriter" />
    internal class RemoveRegionRewriter :
        IRewriter
    {
        /// <summary>
        /// Gets a value indicating whether the project should be recompiled to validate the changes
        /// didn't break the project.
        /// </summary>
        /// <value>
        /// <c>false</c>
        /// </value>
        public bool ShouldRecompileToValidate
            => false;

        /// <summary>
        /// Rewrites the specified document context.
        /// </summary>
        /// <param name="documentContext">The document context.</param>
        /// <param name="log">The log.</param>
        /// <returns></returns>
        public Result<Document> Rewrite(DocumentContext documentContext, ILog log)
        {
            using (log.LogStep($"{nameof(RemoveRegionRewriter)}({documentContext.Name})"))
            {
                var state = new DocumentState(documentContext)
                    .AlterDocumentRoot(root =>
                    {
                        var regions =
                            root
                            .DescendantTrivia(descendIntoTrivia: true)
                            .OfType<SyntaxTrivia>()
                            .Where(x => x.Kind() == SyntaxKind.RegionDirectiveTrivia);

                        var endRegions =
                            root
                            .DescendantTrivia(descendIntoTrivia: true)
                            .OfType<SyntaxTrivia>()
                            .Where(x => x.Kind() == SyntaxKind.EndRegionDirectiveTrivia);

                        var both = regions.Union(endRegions).ToList();
                        var emptyTrivia = new SyntaxTrivia();

                        return root
                            .ReplaceTrivia(both, (_, __) => emptyTrivia);
                    });

                return state.Document;
            }
        }
    }
}