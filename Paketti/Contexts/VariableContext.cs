using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Paketti.Contexts
{
    public class VariableContext
    {
        public VariableDeclaratorSyntax Declaration { get; }
        public CSharpCompilation Compilation { get; }
        public SemanticModel SemanticModel { get; }

        public IFieldSymbol Symbol { get; }

        public VariableContext(VariableDeclaratorSyntax variableDeclarator, CSharpCompilation compilation, SemanticModel semanticModel)
        {
            Declaration = variableDeclarator ?? throw new ArgumentNullException(nameof(variableDeclarator));
            Compilation = compilation ?? throw new ArgumentNullException(nameof(compilation));
            SemanticModel = semanticModel ?? throw new ArgumentNullException(nameof(semanticModel));

            Symbol = (IFieldSymbol)semanticModel.GetDeclaredSymbol(variableDeclarator);
        }

        public string Name
            => Symbol.Name;
    }
}