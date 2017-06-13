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
    public class PropertyContext
    {
        public PropertyDeclarationSyntax Declaration { get; }

        public CSharpCompilation Compilation { get; }

        public SemanticModel SemanticModel { get; }

        public PropertyContext(PropertyDeclarationSyntax propertyDeclaration, CSharpCompilation compilation, SemanticModel semanticModel)
        {
            Declaration = propertyDeclaration ?? throw new ArgumentNullException(nameof(propertyDeclaration));
            Compilation = compilation ?? throw new ArgumentNullException(nameof(compilation));
            SemanticModel = semanticModel ?? throw new ArgumentNullException(nameof(semanticModel));
        }

        public IPropertySymbol Symbol
            => SemanticModel.GetDeclaredSymbol(Declaration);

        public SymbolInfo SymbolInfo
            => SemanticModel.GetSymbolInfo(Declaration);

        public bool HasGet
            => Symbol.GetMethod != null;

        public bool HasSet
            => Symbol.SetMethod != null;

        public string Name
            => Symbol.Name;

        public override string ToString()
            => $"{Declaration.Type} {Name} {{ {(HasGet ? "get; " : string.Empty)}{(HasSet ? "set; " : string.Empty)} }}";

        private string DebuggerDisplay
            => ToString();
    }
}