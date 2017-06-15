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
        /// Gets the type dependencies for a class.
        /// </summary>
        /// <param name="cls">The class.</param>
        /// <returns></returns>
        IEnumerable<TypeContext> GetTypeDependencies(ClassContext cls);

        /// <summary>
        /// Gets the type dependencies for a constructor.
        /// </summary>
        /// <param name="cls">The constructor.</param>
        /// <returns></returns>
        IEnumerable<TypeContext> GetTypeDependencies(ConstructorContext ctr);

        /// <summary>
        /// Gets the type dependencies for a delegate.
        /// </summary>
        /// <param name="cls">The delegate.</param>
        /// <returns></returns>
        IEnumerable<TypeContext> GetTypeDependencies(DelegateContext del);

        /// <summary>
        /// Gets the type dependencies for a method.
        /// </summary>
        /// <param name="cls">The method.</param>
        /// <returns></returns>
        IEnumerable<TypeContext> GetTypeDependencies(MethodContext method);

        /// <summary>
        /// Gets the type dependencies for a property.
        /// </summary>
        /// <param name="cls">The property.</param>
        /// <returns></returns>
        IEnumerable<TypeContext> GetTypeDependencies(PropertyContext property);

        /// <summary>
        /// Gets the type dependencies for a struct.
        /// </summary>
        /// <param name="cls">The class.</param>
        /// <returns></returns>
        IEnumerable<TypeContext> GetTypeDependencies(StructContext str);

        /// <summary>
        /// Gets the type dependencies for a type.
        /// </summary>
        /// <param name="cls">The type.</param>
        /// <returns></returns>
        IEnumerable<TypeContext> GetTypeDependencies(TypeContext type);

        /// <summary>
        /// Gets the type dependencies for a variable/field.
        /// </summary>
        /// <param name="cls">The variable/field.</param>
        /// <returns></returns>
        IEnumerable<TypeContext> GetTypeDependencies(VariableContext variable);
    }
}