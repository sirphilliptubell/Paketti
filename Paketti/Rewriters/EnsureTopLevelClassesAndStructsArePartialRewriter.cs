using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Paketti.Contexts;
using Paketti.Extensions;
using Paketti.Logging;

namespace Paketti.Rewriters
{
    /// <summary>
    /// Modified non-nested classes and structs so they have the partial modifier.
    /// </summary>
    /// <seealso cref="Paketti.Rewriters.IRewriter" />
    internal class EnsureTopLevelClassesAndStructsArePartialRewriter :
        IRewriter
    {
        /// <summary>
        /// Gets a value indicating whether the project should be recompiled to validate the changes
        /// didn't break the project.
        /// </summary>
        /// <value>
        /// <c>true</c>.
        /// </value>
        public bool ShouldRecompileToValidate
            => true;

        /// <summary>
        /// Rewrites the specified document context.
        /// </summary>
        /// <param name="documentContext">The document context.</param>
        /// <param name="log">The log.</param>
        /// <returns></returns>
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