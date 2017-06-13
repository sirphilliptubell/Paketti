using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Paketti.Contexts
{
    public class ConstructorContext
    {
        public ConstructorDeclarationSyntax Declaration { get; }

        public CSharpCompilation Compilation { get; }

        public SemanticModel SemanticModel { get; }

        public IMethodSymbol Symbol { get; }

        public ConstructorContext(ConstructorDeclarationSyntax constructorDeclaration, CSharpCompilation compilation, SemanticModel semanticModel)
        {
            Declaration = constructorDeclaration ?? throw new ArgumentNullException(nameof(constructorDeclaration));
            Compilation = compilation ?? throw new ArgumentException(nameof(compilation));
            SemanticModel = semanticModel ?? throw new ArgumentException(nameof(semanticModel));

            Symbol = semanticModel.GetDeclaredSymbol(constructorDeclaration);
        }
    }
}