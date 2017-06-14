using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Paketti.Contexts;
using Paketti.Logging;

namespace Paketti.Rewriters
{
    internal class RemoveRegionRewriter :
        IRewriter
    {
        public bool ShouldRecompileToValidate
            => false;

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