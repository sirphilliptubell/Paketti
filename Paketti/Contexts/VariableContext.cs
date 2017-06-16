using System;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Paketti.Contexts
{
    /// <summary>
    /// The contextual information for a Variable or Field Declaration.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class VariableContext :
        ITypeDependent
    {
        /// <summary>
        /// Gets the symbol for the variable.
        /// </summary>
        /// <value>
        /// The symbol for the variable.
        /// </value>
        public IFieldSymbol Symbol { get; }

        /// <summary>
        /// Gets the semantic model.
        /// </summary>
        /// <value>
        /// The semantic model.
        /// </value>
        public SemanticModel SemanticModel { get; }

        /// <summary>
        /// Gets the variable declaration.
        /// </summary>
        /// <value>
        /// The variable declaration.
        /// </value>
        public VariableDeclaratorSyntax Declaration { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableContext"/> class.
        /// </summary>
        /// <param name="variableDeclarator">The variable declarator.</param>
        /// <param name="semanticModel">The semantic model.</param>
        /// <exception cref="System.ArgumentNullException">
        /// variableDeclarator
        /// or
        /// semanticModel
        /// </exception>
        public VariableContext(VariableDeclaratorSyntax variableDeclarator, SemanticModel semanticModel)
        {
            Declaration = variableDeclarator ?? throw new ArgumentNullException(nameof(variableDeclarator));
            SemanticModel = semanticModel ?? throw new ArgumentNullException(nameof(semanticModel));

            Symbol = (IFieldSymbol)semanticModel.GetDeclaredSymbol(variableDeclarator);
        }

        /// <summary>
        /// Gets the name of the Symbol.
        /// </summary>
        /// <value>
        /// The name of the Symbol.
        /// </value>
        public string Name
            => Symbol.Name;

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
            => $"{Symbol.Type.ContainingNamespace.Name}.{Symbol.Type.Name} {Name}";

        private string DebuggerDisplay
            => ToString();
    }
}