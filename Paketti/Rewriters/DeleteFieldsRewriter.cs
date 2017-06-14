using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Paketti.Contexts;

namespace Paketti.Rewriters
{
    public class DeleteFieldsRewriter :
        IRewriter
    {
        public bool ShouldRecompileToValidate
            => true;

        public Document Rewrite(DocumentContext document)
        {
            var fields = document
                .GetClassesAndStructs()
                .SelectMany(x => x.Fields)
                .Select(x => x.Declaration)
                .ToList();

            var root = document.SyntaxRoot;
            root = root.RemoveNodes(fields, SyntaxRemoveOptions.KeepNoTrivia);

            return document.Document.WithSyntaxRoot(root);
        }
    }
}