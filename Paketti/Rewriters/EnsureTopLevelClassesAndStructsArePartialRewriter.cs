using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Paketti.Contexts;
using Paketti.Extensions;
using Paketti.Logging;

namespace Paketti.Rewriters
{
    internal class EnsureTopLevelClassesAndStructsArePartialRewriter :
        IRewriter
    {
        public bool ShouldRecompileToValidate
            => true;

        public Result<Document> Rewrite(DocumentContext documentContext, ILog log)
        {
            using (log.LogStep($"{nameof(EnsureTopLevelClassesAndStructsArePartialRewriter)}({documentContext.Name})"))
            {
                return
                    new DocumentState(documentContext)
                    .AlterDocument(doc =>
                    {
                        var old2New = new Dictionary<SyntaxNode, SyntaxNode>();

                        var classesWithoutPartial = doc
                            .GetClassesTopLevel()
                            .Where(x => !x.IsPartial());

                        foreach (var cls in classesWithoutPartial)
                            old2New.Add(cls, cls.AddPartial());

                        var structsWithoutPartial = doc
                            .GetStructsTopLevel()
                            .Where(x => !x.IsPartial());

                        foreach (var str in structsWithoutPartial)
                            old2New.Add(str, str.AddPartial());

                        return
                            doc
                            .AlterRoot(root => root.ReplaceNodes(old2New.Keys, (old, _) => old2New[old]));
                    })
                    .Document;
            }
        }
    }
}