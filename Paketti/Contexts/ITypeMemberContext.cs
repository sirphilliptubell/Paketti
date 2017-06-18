using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Paketti.Contexts
{
    /// <summary>
    /// Represents a Member of a Type, eg: a field, method, property, delegate, or constructor.
    /// </summary>
    public interface ITypeMemberContext
    {
        /// <summary>
        /// Gets the class, struct, or interface that contains this instance.
        /// May be null if this ITypeMemberContext is for a delegate which is not contained
        /// within a type, or if this is for a variable (not a field).
        /// </summary>
        Maybe<ITypeDeclarationContext> ContainingTypeContext { get; }

        /// <summary>
        /// Gets the declaration's syntax node.
        /// </summary>
        /// <value>
        /// The declaration.
        /// </value>
        SyntaxNode Declaration { get; }
    }
}