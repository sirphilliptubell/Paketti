using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Paketti.Contexts
{
    public class TypeContext
    {
        private static IReadOnlyList<string> CLRAssemblyNames = new string[] {
            "System.Runtime",
            "System.Collections",
            "System.Diagnostics.Debug"
        };

        public CSharpCompilation Compilation { get; }

        public SemanticModel SemanticModel { get; }

        public ITypeSymbol Symbol { get; }

        public IEnumerable<TypeContext> TypeArguments { get; }

        public TypeContext(ITypeSymbol typeSymbol, CSharpCompilation compilation, SemanticModel semanticModel)
        {
            Symbol = typeSymbol ?? throw new ArgumentException(nameof(typeSymbol));
            Compilation = compilation ?? throw new ArgumentException(nameof(compilation));
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
                        .Select(x => new TypeContext(x, compilation, semanticModel))
                        .ToList();
                }
                else if (named.IsTupleType)
                    TypeArguments = named
                        .TupleElements
                        .Select(x => new TypeContext(x.Type, compilation, semanticModel))
                        .ToList();
                else
                {
                    TypeArguments = new TypeContext[] { };
                }
            }
            else if (Symbol is IArrayTypeSymbol array)
            {
                TypeArguments = new TypeContext[] { new TypeContext(array.ElementType, compilation, semanticModel) };
            }
            else if (Symbol is ITypeParameterSymbol param)
            {
                TypeArguments = new TypeContext[] { };
            }
            else
                Debugger.Break();
        }

        public TypeContext(TypeSyntax typeSyntax, CSharpCompilation compilation, SemanticModel semanticModel)
            : this(semanticModel.GetTypeInfo(typeSyntax).Type, compilation, semanticModel)
        { }

        public string Name
            => Symbol.Name;

        public string AssemblyName
            // Symbol.ContainingAssembly may be null for if the Symbol is for an Array ("ArrayType")
            => (Symbol.ContainingAssembly ?? Symbol.BaseType.ContainingAssembly).Name;

        public bool IsCLRType
            => CLRAssemblyNames.Contains(AssemblyName);

        public bool IsGeneric
            => Symbol is INamedTypeSymbol named
            && named.IsGenericType;

        /// <summary>
        /// Gets a value indicating whether the type is a generic type parameter, eg: the "T" in List&lt;T&gt; (and not a named type like "int" or "object")
        /// </summary>
        public bool IsGenericParameter
            => Symbol is ITypeParameterSymbol;

        public bool IsArray
            => Symbol is IArrayTypeSymbol;

        public bool IsValueTuple
            => AssemblyName == "System.ValueTuple";

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

        public override int GetHashCode()
            => ToString().GetHashCode();

        public override bool Equals(object obj)
            => ToString().Equals((obj as TypeContext)?.ToString());
    }
}