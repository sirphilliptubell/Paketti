using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paketti.Primitives
{
    /// <summary>
    /// Represents the name of an assembly.
    /// </summary>
    public class AssemblyName
    {
        /// <summary>
        /// Get the Assembly Name for System.ValueTuple.
        /// </summary>
        public static readonly AssemblyName CLR_VALUE_TUPLE = new AssemblyName("System.ValueTuple");

        /// <summary>
        /// Gets the names of Microsoft's CLR assemblies.
        /// </summary>
        private static IReadOnlyList<string> CLRAssemblyNames = new string[] {
            "System.Runtime",
            "System.Collections",
            "System.Diagnostics.Debug"
        };

        private readonly string _value;

        private AssemblyName(string assemblyName)
            => _value = assemblyName;

        /// <summary>
        /// Tries to create the specified assembly name.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">assemblyName</exception>
        public static Maybe<AssemblyName> Create(string assemblyName)
        {
            //todo: add validation
            return new AssemblyName(assemblyName ?? throw new ArgumentNullException(nameof(assemblyName)));
        }

        /// <summary>
        /// Gets a value indicating whether this instance is part of the CLR.
        /// Note: AssemblyNames that are for one of microsoft's packages, eg: ValueTuple / EntityFramework will return false.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is part of the CLR; otherwise, <c>false</c>.
        /// </value>
        public bool IsCLR
            => this != CLR_VALUE_TUPLE
            && CLRAssemblyNames.Any(x => x.ToLowerInvariant() == _value.ToLowerInvariant());

        public static bool operator ==(AssemblyName left, AssemblyName right)
            => left._value.ToLowerInvariant() == right._value.ToLower();

        public static bool operator !=(AssemblyName left, AssemblyName right)
            => !(left == right);

        public override int GetHashCode()
            => _value.ToLowerInvariant().GetHashCode();

        public override bool Equals(object obj)
            => obj is AssemblyName typed && typed == this;

        public override string ToString()
            => _value;
    }
}