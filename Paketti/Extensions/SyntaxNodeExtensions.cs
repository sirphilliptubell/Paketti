using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Paketti.Contexts;

namespace Paketti.Extensions
{
    internal static class SyntaxNodeExtensions
    {
        private const string PakettiExtensionsNamespace = "Paketti.Extensions";
        private const string PakettiNamespace = "Paketti";

        internal static ClassDeclarationSyntax AddModifier(this ClassDeclarationSyntax syntax, SyntaxToken token)
            => syntax.AddModifiers(new SyntaxToken[] { token });

        internal static StructDeclarationSyntax AddModifier(this StructDeclarationSyntax syntax, SyntaxToken token)
            => syntax.AddModifiers(new SyntaxToken[] { token });

        /// <summary>
        /// Adds a "Paketti" namespace to the document, adds the members, and returns the new namespace.
        /// </summary>
        /// <param name="doc">The document.</param>
        /// <param name="members">The members.</param>
        /// <returns></returns>
        internal static Document AddPakettiExtensionsNamespaceAndMembers(this Document doc, IEnumerable<MemberDeclarationSyntax> members)
            => doc.AddNamespaceAndMembers(PakettiExtensionsNamespace, members);

        /// <summary>
        /// Adds a "Paketti" namespace to the document, adds the members, and returns the new namespace.
        /// </summary>
        /// <param name="doc">The document.</param>
        /// <param name="members">The members.</param>
        /// <returns></returns>
        internal static Document AddPakettiNamespaceAndMembers(this Document doc, IEnumerable<MemberDeclarationSyntax> members)
            => doc.AddNamespaceAndMembers(PakettiNamespace, members);

        internal static ClassDeclarationSyntax AddPartial(this ClassDeclarationSyntax syntax)
            => syntax.AddModifier(SyntaxFactory.Token(SyntaxKind.PartialKeyword).WithTrailingTrivia(SyntaxFactory.Space));

        internal static StructDeclarationSyntax AddPartial(this StructDeclarationSyntax syntax)
            => syntax.AddModifier(SyntaxFactory.Token(SyntaxKind.PartialKeyword).WithTrailingTrivia(SyntaxFactory.Space));

        /// <summary>
        /// Creates a new instance of this Document to have the specified changes made to it's root.
        /// </summary>
        /// <param name="doc">The document.</param>
        /// <param name="alter">The method that alters the root.</param>
        /// <returns></returns>
        internal static Document AlterRoot(this Document doc, Func<CompilationUnitSyntax, CompilationUnitSyntax> alter)
                    => doc.WithSyntaxRoot(
                        alter(
                            (CompilationUnitSyntax)doc.GetSyntaxRootAsync().Result)
                        );

        internal static IEnumerable<SyntaxNode> DescendantNodesImmediate(this NamespaceDeclarationSyntax node)
            => node
            .DescendantNodes(descendIntoChildren: n => n is NamespaceDeclarationSyntax);

        internal static IEnumerable<ClassDeclarationSyntax> GetClassesImmediate(this NamespaceDeclarationSyntax node)
            => node
            .DescendantNodesImmediate()
            .OfType<ClassDeclarationSyntax>();

        internal static IEnumerable<ClassDeclarationSyntax> GetClassesTopLevel(this Document document)
            => document
            .GetNamespaces()
            .SelectMany(x => x.GetClassesImmediate());

        internal static (
            IEnumerable<VariableContext> fields,
            IEnumerable<ConstructorContext> constructors,
            IEnumerable<MethodContext> methods,
            IEnumerable<PropertyContext> properties)
            GetDescendantFieldsConstructorsMethodsAndProperties(this SyntaxNode node, SemanticModel semanticModel)
        {
            var fields = node
                .DescendantNodes()
                .OfType<FieldDeclarationSyntax>()
                .SelectMany(x => x.Declaration.Variables)
                .Select(x => new VariableContext(x, semanticModel));

            var constructors = node
                .DescendantNodes()
                .OfType<ConstructorDeclarationSyntax>()
                .Select(x => new ConstructorContext(x, semanticModel));

            var methods = node
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Select(x => new MethodContext(x, semanticModel));

            var properties = node
                .DescendantNodes()
                .OfType<PropertyDeclarationSyntax>()
                .Select(x => new PropertyContext(x, semanticModel));

            return (fields: fields, constructors: constructors, methods: methods, properties: properties);
        }

        internal static IEnumerable<NamespaceDeclarationSyntax> GetNamespaces(this Document document)
            => document
            .GetSyntaxRootAsync().Result
            .DescendantNodes()
            .OfType<NamespaceDeclarationSyntax>();

        internal static Document GetOrCreateDocument(this Project proj, string name, IEnumerable<string> folders)
            => proj
            .Documents.Where(doc => doc.Name == name)
            .SingleOrCreate(() => proj.AddDocument(name, string.Empty, folders));

        //only get the nodes in self
        internal static IEnumerable<StructDeclarationSyntax> GetStructsImmediate(this NamespaceDeclarationSyntax node)
            => node
            .DescendantNodesImmediate()
            .OfType<StructDeclarationSyntax>();

        internal static IEnumerable<StructDeclarationSyntax> GetStructsTopLevel(this Document document)
            => document
            .GetNamespaces()
            .SelectMany(x => x.GetStructsImmediate());

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

        internal static Document RemoveMembers(this Document doc, IEnumerable<MemberDeclarationSyntax> members)
            => doc.AlterRoot(root =>
                root.RemoveNodes(members, SyntaxRemoveOptions.KeepNoTrivia)
            );

        internal static T ReplaceSelf<T>(this SyntaxNode self, T newNode)
                    where T : SyntaxNode
            => (T)self.Map(x => self.ReplaceNode(self, newNode));

        /// <summary>
        /// Gets the string representation of the Syntax and it's trivia
        /// The result is not indented.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        internal static string ToFormattedCode(this SyntaxNode node)
            => Formatter.Format(node, Config.EMPTY_WORKSPACE).ToFullString();

        /// <summary>
        /// Adds a namespace to the document, adds the members, and returns the new namespace.
        /// </summary>
        /// <param name="doc">The document.</param>
        /// <param name="members">The members.</param>
        /// <returns></returns>
        private static Document AddNamespaceAndMembers(this Document doc, string namespaceName, IEnumerable<MemberDeclarationSyntax> members)
            => doc
            .AlterRoot(root =>
                root
                .AddMembers(
                    NewNamespace(namespaceName, members)
                )
            );

        private static NamespaceDeclarationSyntax NewNamespace(string name, IEnumerable<MemberDeclarationSyntax> members)
            => SyntaxFactory.NamespaceDeclaration(
                name: SyntaxFactory.IdentifierName(name),
                externs: SyntaxFactory.List(new ExternAliasDirectiveSyntax[] { }),
                usings: SyntaxFactory.List(new UsingDirectiveSyntax[] { }),
                members: SyntaxFactory.List(members));
    }
}