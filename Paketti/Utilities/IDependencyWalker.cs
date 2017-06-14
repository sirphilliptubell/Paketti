using System.Collections.Generic;
using Paketti.Contexts;

namespace Paketti.Utilities
{
    public interface IDependencyWalker
    {
        IEnumerable<TypeContext> GetTypeDependencies(ClassContext cls);

        IEnumerable<TypeContext> GetTypeDependencies(ConstructorContext ctr);

        IEnumerable<TypeContext> GetTypeDependencies(DelegateContext del);

        IEnumerable<TypeContext> GetTypeDependencies(MethodContext method);

        IEnumerable<TypeContext> GetTypeDependencies(PropertyContext property);

        IEnumerable<TypeContext> GetTypeDependencies(StructContext str);

        IEnumerable<TypeContext> GetTypeDependencies(TypeContext type);

        IEnumerable<TypeContext> GetTypeDependencies(VariableContext variable);
    }
}