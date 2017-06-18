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
    public class ConstructorContext :
        ITypeDependent,
        ITypeMemberContext
    {
        /// <summary>
        /// Gets the constructor's declaration.
        /// </summary>
        /// <value>
        /// The constructor's declaration.
        /// </value>
        public ConstructorDeclarationSyntax Declaration { get; }

        /// <summary>
        /// Gets the declaration's syntax node.
        /// </summary>
        /// <value>
        /// The declaration.
        /// </value>
        SyntaxNode ITypeMemberContext.Declaration => this.Declaration;

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
        /// Gets the class, struct, or interface that contains this instance.
        /// Always has a value.
        /// </summary>
        /// <remarks>
        /// Only a Maybe type because of the interface.
        /// </remarks>
        public Maybe<ITypeDeclarationContext> ContainingTypeContext { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstructorContext" /> class.
        /// </summary>
        /// <param name="containingTypeContext">The context for the type that contains this member.</param>
        /// <param name="constructorDeclaration">The constructor declaration.</param>
        /// <param name="semanticModel">The semantic model.</param>
        /// <exception cref="System.ArgumentNullException">constructorDeclaration</exception>
        /// <exception cref="System.ArgumentException">semanticModel</exception>
        public ConstructorContext(ITypeDeclarationContext containingTypeContext, ConstructorDeclarationSyntax constructorDeclaration, SemanticModel semanticModel)
        {
            if (containingTypeContext == null) throw new ArgumentException(nameof(containingTypeContext));
            ContainingTypeContext = Maybe.From(containingTypeContext);
            Declaration = constructorDeclaration ?? throw new ArgumentNullException(nameof(constructorDeclaration));
            SemanticModel = semanticModel ?? throw new ArgumentException(nameof(semanticModel));

            Symbol = semanticModel.GetDeclaredSymbol(constructorDeclaration);
        }

        public override string ToString()
            => $".ctor for {Symbol.ContainingSymbol.Name}";

        private string DebuggerDisplay
            => nameof(ConstructorContext);
    }
}