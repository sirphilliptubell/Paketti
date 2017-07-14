using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Paketti.Contexts;
using Paketti.Utilities;

namespace Paketti.Extensions
{
    internal static class SyntaxNodeExtensions
    {
        /// <summary>
        /// Adds the specified modifier to the class declaration.
        /// </summary>
        /// <param name="syntax">The syntax.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        internal static ClassDeclarationSyntax AddModifier(this ClassDeclarationSyntax syntax, SyntaxToken token)
            => syntax.AddModifiers(new SyntaxToken[] { token });

        /// <summary>
        /// Adds the specified modifier to the struct declaration
        /// </summary>
        /// <param name="syntax">The syntax.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        internal static StructDeclarationSyntax AddModifier(this StructDeclarationSyntax syntax, SyntaxToken token)
            => syntax.AddModifiers(new SyntaxToken[] { token });

        /// <summary>
        /// Adds the partial modifier to a class declaration and returns the new declaration.
        /// </summary>
        /// <param name="syntax">The syntax.</param>
        /// <returns></returns>
        internal static ClassDeclarationSyntax AddPartial(this ClassDeclarationSyntax syntax)
            => syntax.AddModifier(SyntaxFactory.Token(SyntaxKind.PartialKeyword).WithTrailingTrivia(SyntaxFactory.Space));

        /// <summary>
        /// Adds the partial modifier to a struct declaration and returns the new declaration.
        /// </summary>
        /// <param name="syntax">The syntax.</param>
        /// <returns></returns>
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
                alter(doc.GetRootSync())
                );

        /// <summary>
        /// Gets the root synchronously.
        /// </summary>
        /// <param name="doc">The document.</param>
        /// <returns></returns>
        internal static CompilationUnitSyntax GetRootSync(this Document doc)
            => (CompilationUnitSyntax)doc.GetSyntaxRootAsync().Result;

        /// <summary>
        /// Gets a value indicating whether the document contains the specified node.
        /// </summary>
        /// <param name="doc">The document.</param>
        /// <param name="node">The node.</param>
        /// <returns>
        ///   <c>true</c> if the specified node contains node; otherwise, <c>false</c>.
        /// </returns>
        internal static bool ContainsNode(this Document doc, SyntaxNode node)
            => doc.GetRootSync()
            .DescendantNodes()
            .Where(n => n == node)
            .Any();

        /// <summary>
        /// Gets the immediate descendants of the namespace.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        internal static IEnumerable<SyntaxNode> DescendantNodesImmediate(this NamespaceDeclarationSyntax node)
            => node
            .DescendantNodes(descendIntoChildren: n => n is NamespaceDeclarationSyntax);

        /// <summary>
        /// Gets the top-most classes (not nested within other objects) in the namespace.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        internal static IEnumerable<ClassDeclarationSyntax> GetClassesTopLevel(this NamespaceDeclarationSyntax node)
            => node
            .DescendantNodesImmediate()
            .OfType<ClassDeclarationSyntax>();

        /// <summary>
        /// Gets the top-most classes (not nested within other objects) in the document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns></returns>
        internal static IEnumerable<ClassDeclarationSyntax> GetClassesTopLevel(this Document document)
            => document
            .GetNamespaces()
            .SelectMany(x => x.GetClassesTopLevel());

        /// <summary>
        /// Gets the <c>VariableContext</c>s for the fields in the type.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="semanticModel">The semantic model.</param>
        /// <returns></returns>
        internal static IEnumerable<VariableContext> GetDescendantFieldContexts(this ITypeDeclarationContext typeContext, SemanticModel semanticModel)
            => typeContext.Declaration
            //todo: this may cause issues with nested classes/structs
            .DescendantNodes()
            .OfType<FieldDeclarationSyntax>()
            .SelectMany(x => x.Declaration.Variables)
            .Select(x => new VariableContext(Maybe.From(typeContext), x, semanticModel));

        /// <summary>
        /// Gets the <c>ConstructorContext</c>s for the type.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="semanticModel">The semantic model.</param>
        /// <returns></returns>
        internal static IEnumerable<ConstructorContext> GetDescendantConstructorContexts(this ITypeDeclarationContext typeContext, SemanticModel semanticModel)
            => typeContext.Declaration
            //todo: this may cause issues with nested classes/structs
            .DescendantNodes()
            .OfType<ConstructorDeclarationSyntax>()
            .Select(x => new ConstructorContext(typeContext, x, semanticModel));

        /// <summary>
        /// Gets the <c>MethodContext</c>s for the type.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="semanticModel">The semantic model.</param>
        /// <returns></returns>
        internal static IEnumerable<MethodContext> GetDescendantMethodContexts(this ITypeDeclarationContext typeContext, SemanticModel semanticModel)
            => typeContext.Declaration
            //todo: this may cause issues with nested classes/structs
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Select(x => new MethodContext(typeContext, x, semanticModel));

        /// <summary>
        /// Gets the <c>PropertyContext</c>s for the type.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="semanticModel">The semantic model.</param>
        /// <returns></returns>
        internal static IEnumerable<PropertyContext> GetDescendantPropertyContexts(this ITypeDeclarationContext typeContext, SemanticModel semanticModel)
            => typeContext.Declaration
            //todo: this may cause issues with nested classes/structs
            .DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .Select(x => new PropertyContext(typeContext, x, semanticModel));

        /// <summary>
        /// Gets the namespaces of the document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns></returns>
        internal static IEnumerable<NamespaceDeclarationSyntax> GetNamespaces(this Document document)
            => document
            .GetRootSync()
            .DescendantNodes()
            .OfType<NamespaceDeclarationSyntax>();

        /// <summary>
        /// Gets the top-most structs (not nested within other objects) in the namespace.
        /// </summary>
        /// <param name="node">The namespace's node.</param>
        /// <returns></returns>
        internal static IEnumerable<StructDeclarationSyntax> GetStructsTopLevel(this NamespaceDeclarationSyntax node)
            => node
            .DescendantNodesImmediate()
            .OfType<StructDeclarationSyntax>();

        /// <summary>
        /// Gets the top-most structs (not nested within other objects) in the document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns></returns>
        internal static IEnumerable<StructDeclarationSyntax> GetStructsTopLevel(this Document document)
            => document
            .GetNamespaces()
            .SelectMany(x => x.GetStructsTopLevel());

        /// <summary>
        /// Gets a value indicating whether the class has the partial modifier.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>
        ///   <c>true</c> if the specified class has the partial modifier; otherwise, <c>false</c>.
        /// </returns>
        internal static bool IsPartial(this ClassDeclarationSyntax node)
            => node
            .ChildTokens()
            .Any(x => x.Kind() == SyntaxKind.PartialKeyword);

        /// <summary>
        /// Gets a value indicating whether the struct has the partial modifier.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>
        ///   <c>true</c> if the specified struct has the partial modifier; otherwise, <c>false</c>.
        /// </returns>
        internal static bool IsPartial(this StructDeclarationSyntax node)
            => node
            .ChildTokens()
            .Any(x => x.Kind() == SyntaxKind.PartialKeyword);

        /// <summary>
        /// Removes the specified members from the document.
        /// Does not keep trivia.
        /// </summary>
        /// <param name="doc">The document.</param>
        /// <param name="members">The members.</param>
        /// <returns></returns>
        internal static Document RemoveMembers(this Document doc, IEnumerable<MemberDeclarationSyntax> members)
            => doc.AlterRoot(root =>
                root.RemoveNodes(members, SyntaxRemoveOptions.KeepNoTrivia)
            );

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

        /// <summary>
        /// News the namespace.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="members">The members.</param>
        /// <returns></returns>
        private static NamespaceDeclarationSyntax NewNamespace(string name, IEnumerable<MemberDeclarationSyntax> members)
            => SyntaxFactory.NamespaceDeclaration(
                name: SyntaxFactory.IdentifierName(name),
                externs: Empty.Syntax,
                usings: Empty.Syntax,
                members: SyntaxFactory.List(members));

        /// <summary>
        /// Gets the unused (or duplicate) using directives.
        /// </summary>
        /// <param name="root">The document root.</param>
        /// <returns></returns>
        internal static IEnumerable<SyntaxNode> GetUnusedUsingDirectives(this CompilationUnitSyntax root)
        {
            var result = new HashSet<SyntaxNode>();

            var diagnostics = root
                .GetDiagnostics()
                .Where(d => d.Id == DiagnosticId.DUPLICATE_USING_DIRECTIVE || d.Id == DiagnosticId.UNNECESSARY_USING_DIRECTIVE);

            foreach (var diagnostic in diagnostics)
            {
                if (root.FindNode(diagnostic.Location.SourceSpan) is UsingDirectiveSyntax syntax)
                {
                    result.Add(syntax);
                }
            }

            return result;
        }
    }
}