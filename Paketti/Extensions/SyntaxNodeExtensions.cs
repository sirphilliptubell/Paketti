using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Paketti.Contexts;

namespace Paketti
{
    internal static class SyntaxNodeExtensions
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

        internal static bool IsPartial(this ClassDeclarationSyntax node)
            => node
            .ChildTokens()
            .Any(x => x.Kind() == SyntaxKind.PartialKeyword);

        internal static bool IsPartial(this StructDeclarationSyntax node)
            => node
            .ChildTokens()
            .Any(x => x.Kind() == SyntaxKind.PartialKeyword);

        internal static bool IsPartial(this NamespaceDeclarationSyntax node)
            => node
            .ChildTokens()
            .Any(x => x.Kind() == SyntaxKind.PartialKeyword);

        internal static IEnumerable<SyntaxNode> DescendantNodesImmediate(this NamespaceDeclarationSyntax node)
            => node
            .DescendantNodes(descendIntoChildren: n => n is NamespaceDeclarationSyntax); //only get the nodes in self

        internal static IEnumerable<NamespaceDeclarationSyntax> GetNamespaces(this Document document)
            => document
            .GetSyntaxRootAsync().Result
            .DescendantNodes()
            .OfType<NamespaceDeclarationSyntax>();

        internal static IEnumerable<ClassDeclarationSyntax> GetClassesImmediate(this NamespaceDeclarationSyntax node)
            => node
            .DescendantNodesImmediate()
            .OfType<ClassDeclarationSyntax>();

        internal static IEnumerable<StructDeclarationSyntax> GetStructsImmediate(this NamespaceDeclarationSyntax node)
            => node
            .DescendantNodesImmediate()
            .OfType<StructDeclarationSyntax>();

        internal static IEnumerable<ClassDeclarationSyntax> GetClassesTopLevel(this Document document)
            => document
            .GetNamespaces()
            .SelectMany(x => x.GetClassesImmediate());

        internal static IEnumerable<StructDeclarationSyntax> GetStructsTopLevel(this Document document)
            => document
            .GetNamespaces()
            .SelectMany(x => x.GetStructsImmediate());

        internal static ClassDeclarationSyntax AddModifier(this ClassDeclarationSyntax syntax, SyntaxToken token)
            => syntax.AddModifiers(new SyntaxToken[] { token });

        internal static StructDeclarationSyntax AddModifier(this StructDeclarationSyntax syntax, SyntaxToken token)
            => syntax.AddModifiers(new SyntaxToken[] { token });

        internal static ClassDeclarationSyntax AddPartial(this ClassDeclarationSyntax syntax)
            => syntax.AddModifier(SyntaxFactory.Token(SyntaxKind.PartialKeyword).WithTrailingTrivia(SyntaxFactory.Space));

        internal static StructDeclarationSyntax AddPartial(this StructDeclarationSyntax syntax)
            => syntax.AddModifier(SyntaxFactory.Token(SyntaxKind.PartialKeyword).WithTrailingTrivia(SyntaxFactory.Space));
    }
}