using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
                    .AlterDocumentContext(dc =>
                    {
                        var root = dc.Document.GetRootSync();

                        var oldUsings = root
                            .DescendantNodes()
                            .OfType<UsingDirectiveSyntax>();

                        var usingsToRemove = dc.SemanticModel
                            .GetDiagnostics()
                            .Where(x => x.Id == DiagnosticId.DUPLICATE_USING_DIRECTIVE || x.Id == DiagnosticId.UNNECESSARY_USING_DIRECTIVE)
                            .Select(x => dc.Document.GetRootSync().FindNode(x.Location.SourceSpan) as UsingDirectiveSyntax)
                            .Where(x => x != null);

                        var newUsings = oldUsings.Except(usingsToRemove);

                        var newRoot = root.WithUsings(SyntaxFactory.List(newUsings));

                        return dc.Document.WithSyntaxRoot(newRoot);
                    });

                return state.Value.Document;
            }
        }
    }
}