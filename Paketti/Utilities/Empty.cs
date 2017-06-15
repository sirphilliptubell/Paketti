using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Paketti.Utilities
{
    /// <summary>
    /// Static class for getting empty types of Syntax objects.
    /// </summary>
    internal class Empty
    {
        /// <summary>
        /// Gets an empty Syntax.
        /// </summary>
        public static readonly Empty Syntax = new Empty();

        private static readonly SyntaxList<UsingDirectiveSyntax> _listOfUsing = SyntaxFactory.List(new UsingDirectiveSyntax[] { });
        private static readonly SyntaxList<ExternAliasDirectiveSyntax> _listOfExternAlias = SyntaxFactory.List(new ExternAliasDirectiveSyntax[] { });

        public static implicit operator SyntaxList<UsingDirectiveSyntax>(Empty empty) => _listOfUsing;

        public static implicit operator SyntaxList<ExternAliasDirectiveSyntax>(Empty empty) => _listOfExternAlias;
    }
}