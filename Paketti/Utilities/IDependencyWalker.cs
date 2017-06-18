using System.Collections.Generic;
using Paketti.Contexts;

namespace Paketti.Utilities
{
    /// <summary>
    /// Gets Type dependencies for various Syntax Contexts.
    /// </summary>
    public interface IDependencyWalker
    {
        /// <summary>
        /// Gets the project the walker is assigned to.
        /// </summary>
        /// <value>
        /// The project context.
        /// </value>
        ProjectContext ProjectContext { get; }

        /// <summary>
        /// Gets the type dependencies for a class.
        /// </summary>
        /// <param name="cls">The class.</param>
        /// <param name="visited">The set of type dependent symbols which have already been visited.</param>
        /// <returns></returns>
        IEnumerable<TypeContext> GetTypeDependencies(ClassContext cls, HashSet<ITypeDependent> visited = null);

        /// <summary>
        /// Gets the type dependencies for a constructor.
        /// </summary>
        /// <param name="cls">The constructor.</param>
        /// <param name="visited">The set of type dependent symbols which have already been visited.</param>
        /// <returns></returns>
        IEnumerable<TypeContext> GetTypeDependencies(ConstructorContext ctr, HashSet<ITypeDependent> visited = null);

        /// <summary>
        /// Gets the type dependencies for a delegate.
        /// </summary>
        /// <param name="cls">The delegate.</param>
        /// <param name="visited">The set of type dependent symbols which have already been visited.</param>
        /// <returns></returns>
        IEnumerable<TypeContext> GetTypeDependencies(DelegateContext del, HashSet<ITypeDependent> visited = null);

        /// <summary>
        /// Gets the type dependencies for a method.
        /// </summary>
        /// <param name="cls">The method.</param>
        /// <param name="visited">The set of type dependent symbols which have already been visited.</param>
        /// <returns></returns>
        IEnumerable<TypeContext> GetTypeDependencies(MethodContext method, HashSet<ITypeDependent> visited = null);

        /// <summary>
        /// Gets the type dependencies for a property.
        /// </summary>
        /// <param name="cls">The property.</param>
        /// <param name="visited">The set of type dependent symbols which have already been visited.</param>
        /// <returns></returns>
        IEnumerable<TypeContext> GetTypeDependencies(PropertyContext property, HashSet<ITypeDependent> visited = null);

        /// <summary>
        /// Gets the type dependencies for a struct.
        /// </summary>
        /// <param name="cls">The class.</param>
        /// <param name="visited">The set of type dependent symbols which have already been visited.</param>
        /// <returns></returns>
        IEnumerable<TypeContext> GetTypeDependencies(StructContext str, HashSet<ITypeDependent> visited = null);

        /// <summary>
        /// Gets the type dependencies for a type declaration.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        IEnumerable<TypeContext> GetTypeDependencies(ITypeDeclarationContext value);

        /// <summary>
        /// Gets the type dependencies for a type member.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        IEnumerable<TypeContext> GetTypeDependencies(ITypeMemberContext member);

        /// <summary>
        /// Gets the type dependencies for a type.
        /// </summary>
        /// <param name="cls">The type.</param>
        /// <param name="visited">The set of type dependent symbols which have already been visited.</param>
        /// <returns></returns>
        IEnumerable<TypeContext> GetTypeDependencies(TypeContext type, HashSet<ITypeDependent> visited = null);

        /// <summary>
        /// Gets the type dependencies for a variable/field.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <param name="visited">The set of type dependent symbols which have already been visited.</param>
        /// <returns></returns>
        IEnumerable<TypeContext> GetTypeDependencies(VariableContext variable, HashSet<ITypeDependent> visited = null);
    }
}