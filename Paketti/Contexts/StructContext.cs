using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Paketti.Contexts
{
    public class StructContext :
        IClassOrStruct
    {
        public StructDeclarationSyntax Declaration { get; }

        public CSharpCompilation Compilation { get; }

        public SemanticModel SemanticModel { get; }

        public INamedTypeSymbol Symbol { get; }

        public StructContext(StructDeclarationSyntax structDeclaration, CSharpCompilation compilation, SemanticModel semanticModel)
        {
            Declaration = structDeclaration ?? throw new ArgumentNullException(nameof(structDeclaration));
            Compilation = compilation ?? throw new ArgumentException(nameof(compilation));
            SemanticModel = semanticModel ?? throw new ArgumentException(nameof(semanticModel));

            Symbol = semanticModel.GetDeclaredSymbol(structDeclaration);

            var contexts = structDeclaration.GetDescendantFieldsConstructorsMethodsAndProperties(compilation, semanticModel);

            Properties = contexts.properties.ToList();
            Methods = contexts.methods.ToList();
            Fields = contexts.fields.ToList();
            Constructors = contexts.constructors.ToList();
        }

        public IEnumerable<PropertyContext> Properties { get; }

        public IEnumerable<MethodContext> Methods { get; }

        public IEnumerable<VariableContext> Fields { get; }

        public IEnumerable<ConstructorContext> Constructors { get; }

        public bool IsStatic
            => Symbol.IsStatic;

        public bool IsPartial
            => Declaration.IsPartial();

        public override string ToString()
        {
            var stat = Symbol.IsStatic ? "static " : string.Empty;
            return $"{stat}struct {Symbol.Name}";
        }
    }
}