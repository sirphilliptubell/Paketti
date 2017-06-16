using System;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Paketti.Contexts
{
    /// <summary>
    /// The contextual information for a PropertyDeclaration.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class PropertyContext :
        ITypeDependent
    {
        /// <summary>
        /// Gets the property declaration.
        /// </summary>
        /// <value>
        /// The property declaration.
        /// </value>
        public PropertyDeclarationSyntax Declaration { get; }

        /// <summary>
        /// Gets the semantic model.
        /// </summary>
        /// <value>
        /// The semantic model.
        /// </value>
        public SemanticModel SemanticModel { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyContext"/> class.
        /// </summary>
        /// <param name="propertyDeclaration">The property declaration.</param>
        /// <param name="semanticModel">The semantic model.</param>
        /// <exception cref="System.ArgumentNullException">
        /// propertyDeclaration
        /// or
        /// semanticModel
        /// </exception>
        public PropertyContext(PropertyDeclarationSyntax propertyDeclaration, SemanticModel semanticModel)
        {
            Declaration = propertyDeclaration ?? throw new ArgumentNullException(nameof(propertyDeclaration));
            SemanticModel = semanticModel ?? throw new ArgumentNullException(nameof(semanticModel));
        }

        /// <summary>
        /// Gets a value indicating whether this instance has a get method.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has a get method; otherwise, <c>false</c>.
        /// </value>
        public bool HasGet
            => Symbol.GetMethod != null;

        /// <summary>
        /// Gets a value indicating whether this instance has a set method.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has a set method; otherwise, <c>false</c>.
        /// </value>
        public bool HasSet
            => Symbol.SetMethod != null;

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        /// <value>
        /// The name of the property.
        /// </value>
        public string Name
            => Symbol.Name;

        /// <summary>
        /// Gets the symbol of the property.
        /// </summary>
        /// <value>
        /// The symbol of the property.
        /// </value>
        public IPropertySymbol Symbol
            => SemanticModel.GetDeclaredSymbol(Declaration);

        /// <summary>
        /// Gets the symbol information of the property.
        /// </summary>
        /// <value>
        /// The symbol information of the property.
        /// </value>
        public SymbolInfo SymbolInfo
            => SemanticModel.GetSymbolInfo(Declaration);

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
            => $"{Declaration.Type} {Name} {{ {(HasGet ? "get; " : string.Empty)}{(HasSet ? "set; " : string.Empty)} }}";

        private string DebuggerDisplay
            => ToString();
    }
}