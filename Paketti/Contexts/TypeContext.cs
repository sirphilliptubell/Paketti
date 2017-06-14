using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Paketti.Extensions;

namespace Paketti.Contexts
{
    /// <summary>
    /// The contextual information for a Type Syntax or Symbol.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class TypeContext
    {
        /// <summary>
        /// Gets the names of Microsoft's CLR assemblies.
        /// </summary>
        private static IReadOnlyList<string> CLRAssemblyNames = new string[] {
            "System.Runtime",
            "System.Collections",
            "System.Diagnostics.Debug"
        };

        /// <summary>
        /// Gets the generic type arguments, if any.
        /// </summary>
        /// <value>
        /// The generic type arguments, if any.
        /// </value>
        public IEnumerable<TypeContext> TypeArguments { get; }

        /// <summary>
        /// Gets the symbol for the type.
        /// </summary>
        /// <value>
        /// The symbol for the type.
        /// </value>
        public ITypeSymbol Symbol { get; }

        /// <summary>
        /// Gets the semantic model.
        /// </summary>
        /// <value>
        /// The semantic model.
        /// </value>
        public SemanticModel SemanticModel { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeContext"/> class.
        /// </summary>
        /// <param name="typeSymbol">The type symbol.</param>
        /// <param name="semanticModel">The semantic model.</param>
        /// <exception cref="System.ArgumentException">
        /// typeSymbol
        /// or
        /// semanticModel
        /// or
        /// TypeContext
        /// </exception>
        public TypeContext(ITypeSymbol typeSymbol, SemanticModel semanticModel)
        {
            Symbol = typeSymbol ?? throw new ArgumentException(nameof(typeSymbol));
            SemanticModel = semanticModel ?? throw new ArgumentException(nameof(semanticModel));
            if (typeSymbol.Kind == SymbolKind.ErrorType) throw new ArgumentException($"Cannot create a {nameof(TypeContext)} for {nameof(SymbolKind)}.{nameof(SymbolKind.ErrorType)}");

            /* Possible Types:
             *  INamedTypeSymbol
             *  IArrayTypeSymbol
             *  ITypeParameterSymbol
             */
            if (Symbol is INamedTypeSymbol named)
            {
                if (named.IsGenericType)
                {
                    TypeArguments = named
                        .TypeArguments
                        .Select(x => new TypeContext(x, semanticModel))
                        .ToList();
                }
                else if (named.IsTupleType)
                    TypeArguments = named
                        .TupleElements
                        .Select(x => new TypeContext(x.Type, semanticModel))
                        .ToList();
                else
                {
                    TypeArguments = new TypeContext[] { };
                }
            }
            else if (Symbol is IArrayTypeSymbol array)
            {
                TypeArguments = new TypeContext[] { new TypeContext(array.ElementType, semanticModel) };
            }
            else if (Symbol is ITypeParameterSymbol param)
            {
                TypeArguments = new TypeContext[] { };
            }
            else
                Debugger.Break();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeContext"/> class.
        /// </summary>
        /// <param name="typeSyntax">The type syntax.</param>
        /// <param name="semanticModel">The semantic model.</param>
        public TypeContext(TypeSyntax typeSyntax, SemanticModel semanticModel)
            : this(semanticModel.GetTypeInfo(typeSyntax).Type, semanticModel)
        { }

        /// <summary>
        /// Gets the name of the assembly.
        /// </summary>
        /// <value>
        /// The name of the assembly.
        /// </value>
        public string AssemblyName
            // Symbol.ContainingAssembly may be null for if the Symbol is for an Array ("ArrayType")
            => (Symbol.ContainingAssembly ?? Symbol.BaseType.ContainingAssembly).Name;

        /// <summary>
        /// Gets a value indicating whether this instance is an array.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is an array; otherwise, <c>false</c>.
        /// </value>
        public bool IsArray
            => Symbol is IArrayTypeSymbol;

        /// <summary>
        /// Gets a value indicating whether this instance appears to be a .Net CLR type.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance appears to be a .Net CLR type; otherwise, <c>false</c>.
        /// </value>
        public bool IsCLRType
            => CLRAssemblyNames.Contains(AssemblyName);

        /// <summary>
        /// Gets a value indicating whether this instance is generic.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is generic; otherwise, <c>false</c>.
        /// </value>
        public bool IsGeneric
            => Symbol is INamedTypeSymbol named
            && named.IsGenericType;

        /// <summary>
        /// Gets a value indicating whether the type is a generic type parameter.
        /// eg: the "T" in List&lt;T&gt; (and not a named type like "int" or "object")
        /// </summary>
        public bool IsGenericParameter
            => Symbol is ITypeParameterSymbol;

        /// <summary>
        /// Gets a value indicating whether this instance is System.ValueTuple.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is System.ValueTuple; otherwise, <c>false</c>.
        /// </value>
        public bool IsValueTuple
            => AssemblyName == "System.ValueTuple";

        /// <summary>
        /// Gets the name of the Symbol.
        /// </summary>
        /// <value>
        /// The name of the Symbol.
        /// </value>
        public string Name
            => Symbol.Name;

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
            => ToString().Equals((obj as TypeContext)?.ToString());

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
            => ToString().GetHashCode();

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var generic
                = IsGeneric
                ? $"<{TypeArguments.Select(x => x.ToString()).ToCommaSeparated()}>"
                : string.Empty;

            var vTuple
                = IsValueTuple
                ? $"ValueTuple<{TypeArguments.Select(x => x.ToString()).ToCommaSeparated()}>"
                : string.Empty;

            var array
                = IsArray
                ? $"{TypeArguments.Single().Name}[]"
                : string.Empty;

            return $"{AssemblyName}.{Name}{generic}{vTuple}{array}";
        }

        private string DebuggerDisplay
            => ToString();
    }
}