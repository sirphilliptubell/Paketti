using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Paketti.Contexts;

namespace Paketti.Rewriters
{
    public partial class EnsureTopLevelClassesAndStructsArePartial :
        IRewriter
    {
        public bool ShouldRecompileToValidate
            => true;

        public Document Rewrite(DocumentContext documentContext)
        {
            var old2New = new Dictionary<SyntaxNode, SyntaxNode>();

            var classesWithoutPartial = documentContext.Document
                .GetClassesTopLevel()
                .Where(x => !x.IsPartial());

            foreach (var cls in classesWithoutPartial)
                old2New.Add(cls, cls.AddPartial());

            var structsWithoutPartial = documentContext.Document
                .GetStructsTopLevel()
                .Where(x => !x.IsPartial());

            foreach (var str in structsWithoutPartial)
                old2New.Add(str, str.AddPartial());

            var newRoot = documentContext.Document
                .GetSyntaxRootAsync().Result
                .ReplaceNodes(old2New.Keys, (old, _) => old2New[old]);

            var result = documentContext.Document
                .WithSyntaxRoot(newRoot);

            return result;
        }
    }
}