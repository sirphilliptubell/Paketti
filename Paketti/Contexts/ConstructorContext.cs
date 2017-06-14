using System;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Paketti.Contexts
{
    /// <summary>
    /// The contextual information for a ConstructorDeclaration.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class ConstructorContext
    {
        /// <summary>
        /// Gets the constructor's declaration.
        /// </summary>
        /// <value>
        /// The constructor's declaration.
        /// </value>
        public ConstructorDeclarationSyntax Declaration { get; }

        /// <summary>
        /// Gets the symbol for the constructor.
        /// </summary>
        /// <value>
        /// The symbol for the constructor.
        /// </value>
        public IMethodSymbol Symbol { get; }

        /// <summary>
        /// Gets the semantic model.
        /// </summary>
        /// <value>
        /// The semantic model.
        /// </value>
        public SemanticModel SemanticModel { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstructorContext"/> class.
        /// </summary>
        /// <param name="constructorDeclaration">The constructor declaration.</param>
        /// <param name="semanticModel">The semantic model.</param>
        /// <exception cref="System.ArgumentNullException">constructorDeclaration</exception>
        /// <exception cref="System.ArgumentException">semanticModel</exception>
        public ConstructorContext(ConstructorDeclarationSyntax constructorDeclaration, SemanticModel semanticModel)
        {
            Declaration = constructorDeclaration ?? throw new ArgumentNullException(nameof(constructorDeclaration));
            SemanticModel = semanticModel ?? throw new ArgumentException(nameof(semanticModel));

            Symbol = semanticModel.GetDeclaredSymbol(constructorDeclaration);
        }

        private string DebuggerDisplay
            => nameof(ConstructorContext);
    }
}