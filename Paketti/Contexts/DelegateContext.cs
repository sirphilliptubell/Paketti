using System;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Paketti.Contexts
{
    /// <summary>
    /// The contextual information for a DelegateDeclaration.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class DelegateContext :
        ITypeDependent,
        ITypeMemberContext
    {
        /// <summary>
        /// Gets the delegate's declaration.
        /// </summary>
        /// <value>
        /// The delegate's declaration.
        /// </value>
        public DelegateDeclarationSyntax Declaration { get; }

        /// <summary>
        /// Gets the declaration's syntax node.
        /// </summary>
        /// <value>
        /// The declaration.
        /// </value>
        SyntaxNode ITypeMemberContext.Declaration => this.Declaration;

        /// <summary>
        /// Gets the symbol for the delegate.
        /// </summary>
        /// <value>
        /// The symbol for the delegate.
        /// </value>
        public INamedTypeSymbol Symbol { get; }

        /// <summary>
        /// Gets the semantic model.
        /// </summary>
        /// <value>
        /// The semantic model.
        /// </value>
        public SemanticModel SemanticModel { get; }

        /// <summary>
        /// Gets the class, struct, or interface that contains this instance.
        /// May be null if the delegate was not defined within a class/struct/interface.
        /// </summary>
        public Maybe<ITypeDeclarationContext> ContainingTypeContext { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateContext"/> class.
        /// </summary>
        /// <param name="containingTypeContext">The context for the type that contains this member.
        /// May be null if the delegate was not defined within a class/struct/interface.</param>
        /// <param name="delegateDeclaration">The delegate declaration.</param>
        /// <param name="semanticModel">The semantic model.</param>
        /// <exception cref="System.ArgumentNullException">
        /// delegateDeclaration
        /// or
        /// semanticModel
        /// </exception>
        public DelegateContext(Maybe<ITypeDeclarationContext> containingTypeContext, DelegateDeclarationSyntax delegateDeclaration, SemanticModel semanticModel)
        {
            ContainingTypeContext = containingTypeContext;
            Declaration = delegateDeclaration ?? throw new ArgumentNullException(nameof(delegateDeclaration));
            SemanticModel = semanticModel ?? throw new ArgumentNullException(nameof(semanticModel));

            Symbol = semanticModel.GetDeclaredSymbol(delegateDeclaration);
        }

        private string DebuggerDisplay
            => nameof(DelegateContext);
    }
}