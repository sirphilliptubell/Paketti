using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Paketti.Contexts
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class MethodContext
    {
        public MethodDeclarationSyntax Declaration { get; }

        public CSharpCompilation Compilation { get; }

        public SemanticModel SemanticModel { get; }

        public IMethodSymbol Symbol { get; }

        public MethodContext(MethodDeclarationSyntax methodDeclaration, CSharpCompilation compilation, SemanticModel semanticModel)
        {
            Declaration = methodDeclaration ?? throw new ArgumentNullException(nameof(methodDeclaration));
            Compilation = compilation ?? throw new ArgumentException(nameof(compilation));
            SemanticModel = semanticModel ?? throw new ArgumentException(nameof(semanticModel));

            Symbol = semanticModel.GetDeclaredSymbol(methodDeclaration);
        }

        public string Name
            => Symbol.Name;

        public bool IsExtensionMethod
            => Symbol.IsExtensionMethod;

        public override string ToString()
            => $"{Declaration.ReturnType} {Name}";

        private string DebuggerDisplay
            => ToString();
    }
}