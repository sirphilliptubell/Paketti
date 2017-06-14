﻿using System;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Paketti.Extensions;

namespace Paketti.Contexts
{
    /// <summary>
    /// The contextual information for a MethodDeclaration.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class MethodContext
    {
        /// <summary>
        /// Gets the symbol of the method.
        /// </summary>
        /// <value>
        /// The symbol of the method.
        /// </value>
        public IMethodSymbol Symbol { get; }

        /// <summary>
        /// Gets the method declaration.
        /// </summary>
        /// <value>
        /// The method declaration.
        /// </value>
        public MethodDeclarationSyntax Declaration { get; }

        /// <summary>
        /// Gets the semantic model.
        /// </summary>
        /// <value>
        /// The semantic model.
        /// </value>
        public SemanticModel SemanticModel { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodContext"/> class.
        /// </summary>
        /// <param name="methodDeclaration">The method declaration.</param>
        /// <param name="semanticModel">The semantic model.</param>
        /// <exception cref="System.ArgumentNullException">methodDeclaration</exception>
        /// <exception cref="System.ArgumentException">semanticModel</exception>
        public MethodContext(MethodDeclarationSyntax methodDeclaration, SemanticModel semanticModel)
        {
            Declaration = methodDeclaration ?? throw new ArgumentNullException(nameof(methodDeclaration));
            SemanticModel = semanticModel ?? throw new ArgumentException(nameof(semanticModel));

            Symbol = semanticModel.GetDeclaredSymbol(methodDeclaration);
        }

        /// <summary>
        /// Gets a value indicating whether this instance is an extension method.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is extension method; otherwise, <c>false</c>.
        /// </value>
        public bool IsExtensionMethod
            => Symbol.IsExtensionMethod;

        /// <summary>
        /// Gets the name of the method.
        /// </summary>
        /// <value>
        /// The name of the method.
        /// </value>
        public string Name
            => Symbol.Name;

        /// <summary>
        /// Gets the string representation of the method and it's trivia.
        /// Result is not indented.
        /// </summary>
        /// <returns></returns>
        public string ToFormattedCode()
            => Declaration.ToFormattedCode();

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
            => $"{Declaration.ReturnType} {Name}({Symbol.Parameters.Length} parameters)";

        private string DebuggerDisplay
            => ToString();
    }
}