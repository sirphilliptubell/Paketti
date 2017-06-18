using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Paketti.Contexts
{
    /// <summary>
    /// Represents the contextual information for a class, struct, or interface.
    /// </summary>
    public interface ITypeDeclarationContext :
        IKeyable
    {
        /// <summary>
        /// Gets the type declaration syntax.
        /// </summary>
        /// <value>
        /// The declaration.
        /// </value>
        TypeDeclarationSyntax Declaration { get; }

        /// <summary>
        /// Gets the constructors in the class/struct.
        /// </summary>
        /// <value>
        /// The constructors in the class/struct.
        /// </value>
        IEnumerable<ConstructorContext> Constructors { get; }

        /// <summary>
        /// Gets the methods in the class/struct.
        /// </summary>
        /// <value>
        /// The methods in the class/struct.
        /// </value>
        IEnumerable<MethodContext> Methods { get; }

        /// <summary>
        /// Gets the properties in the class/struct.
        /// </summary>
        /// <value>
        /// The properties in the class/struct.
        /// </value>
        IEnumerable<PropertyContext> Properties { get; }

        /// <summary>
        /// Gets the fields in the class/struct.
        /// </summary>
        /// <value>
        /// The fields in the class/struct.
        /// </value>
        IEnumerable<VariableContext> Fields { get; }
    }
}