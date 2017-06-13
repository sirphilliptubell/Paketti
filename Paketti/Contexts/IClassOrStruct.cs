using System;
using System.Collections.Generic;
using System.Text;

namespace Paketti.Contexts
{
    public interface IClassOrStruct
    {
        IEnumerable<PropertyContext> Properties { get; }

        IEnumerable<MethodContext> Methods { get; }

        IEnumerable<VariableContext> Fields { get; }

        IEnumerable<ConstructorContext> Constructors { get; }
    }
}