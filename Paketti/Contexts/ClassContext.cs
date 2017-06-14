using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Paketti.Contexts
{
    public class ClassContext :
        IClassOrStruct
    {
        public ClassDeclarationSyntax Declaration { get; }

        public CSharpCompilation Compilation { get; }

        public SemanticModel SemanticModel { get; }

        public ClassContext(ClassDeclarationSyntax classDeclaration, CSharpCompilation compilation, SemanticModel semanticModel)
        {
            Declaration = classDeclaration ?? throw new ArgumentNullException(nameof(classDeclaration));
            Compilation = compilation ?? throw new ArgumentException(nameof(compilation));
            SemanticModel = semanticModel ?? throw new ArgumentException(nameof(semanticModel));

            Symbol = semanticModel.GetDeclaredSymbol(classDeclaration);

            var contexts = classDeclaration.GetDescendantFieldsConstructorsMethodsAndProperties(compilation, semanticModel);

            Properties = contexts.properties.ToList();
            Methods = contexts.methods.ToList();
            Fields = contexts.fields.ToList();
            Constructors = contexts.constructors.ToList();
        }

        public INamedTypeSymbol Symbol { get; }

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
            var stat = IsStatic ? "static " : string.Empty;
            return $"{stat}class {Symbol.Name}";
        }
    }
}