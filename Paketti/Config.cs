using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Paketti
{
    internal static class Config
    {
        internal static readonly IReadOnlyCollection<string> PACKED_EXTENSIONS_FOLDER = new string[] { "Extensions" };

        internal static readonly AdhocWorkspace EMPTY_WORKSPACE = new AdhocWorkspace();

        internal static readonly string PACKAGE_LINE_SEPARATOR = string.Empty;

        internal static readonly NamespaceDeclarationSyntax PACKED_EXTENSIONS_NAMESPACE
            = SyntaxFactory.NamespaceDeclaration(
                SyntaxFactory.QualifiedName(
                    SyntaxFactory.IdentifierName("Packed"),
                    SyntaxFactory.IdentifierName("Extensions")
                    )
                );
    }
}