using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Paketti.Contexts
{
    public class DelegateContext
    {
        public DelegateDeclarationSyntax Declaration { get; }

        public CSharpCompilation Compilation { get; }

        public SemanticModel SemanticModel { get; }

        public INamedTypeSymbol Symbol { get; }

        public DelegateContext(DelegateDeclarationSyntax delegateDeclaration, CSharpCompilation compilation, SemanticModel semanticModel)
        {
            Declaration = delegateDeclaration ?? throw new ArgumentNullException(nameof(delegateDeclaration));
            Compilation = compilation ?? throw new ArgumentNullException(nameof(compilation));
            SemanticModel = semanticModel ?? throw new ArgumentNullException(nameof(semanticModel));

            Symbol = semanticModel.GetDeclaredSymbol(delegateDeclaration);
        }
    }
}