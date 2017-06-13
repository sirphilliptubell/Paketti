using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Paketti.Contexts;

namespace Paketti
{
    internal static class NodeSyntaxExtensions
    {
        internal static (IEnumerable<VariableContext> fields, IEnumerable<ConstructorContext> constructors, IEnumerable<MethodContext> methods, IEnumerable<PropertyContext> properties)
            GetDescendantFieldsConstructorsMethodsAndProperties(this SyntaxNode node, CSharpCompilation compilation, SemanticModel semanticModel)
        {
            var fields = node
                .DescendantNodes()
                .OfType<FieldDeclarationSyntax>()
                .SelectMany(x => x.Declaration.Variables)
                .Select(x => new VariableContext(x, compilation, semanticModel));

            var constructors = node
                .DescendantNodes()
                .OfType<ConstructorDeclarationSyntax>()
                .Select(x => new ConstructorContext(x, compilation, semanticModel));

            var methods = node
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Select(x => new MethodContext(x, compilation, semanticModel));

            var properties = node
                .DescendantNodes()
                .OfType<PropertyDeclarationSyntax>()
                .Select(x => new PropertyContext(x, compilation, semanticModel));

            return (fields: fields, constructors: constructors, methods: methods, properties: properties);
        }
    }
}