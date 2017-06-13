using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Paketti.Contexts;

namespace Paketti.Rewriters
{
    public class DeleteExtensionMethodsRewriter :
        IRewriter
    {
        public bool ShouldRecompileToValidate
            => true;

        public async Task<Document> Rewrite(DocumentContext document)
        {
            var extensions = document.GetExtensionMethods().Select(x => x.Declaration).ToList();
            var root = document.SyntaxRoot;
            root = root.RemoveNodes(extensions, SyntaxRemoveOptions.KeepNoTrivia);

            return document.Document.WithSyntaxRoot(root);
        }
    }
}