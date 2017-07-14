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
    /// The contextual information for a StructDeclaration.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class StructContext :
        ITypeDependent,
        ITypeDeclarationContext
    {
        /// <summary>
        /// Gets the struct declaration.
        /// </summary>
        /// <value>
        /// The struct declaration.
        /// </value>
        public StructDeclarationSyntax Declaration { get; }

        /// <summary>
        /// Gets the type declaration syntax.
        /// </summary>
        /// <value>
        /// The declaration.
        /// </value>
        TypeDeclarationSyntax ITypeDeclarationContext.Declaration => this.Declaration;

        /// <summary>
        /// Gets the constructors in the struct.
        /// </summary>
        /// <value>
        /// The constructors in the struct.
        /// </value>
        public IEnumerable<ConstructorContext> Constructors { get; }

        /// <summary>
        /// Gets the methods in the struct.
        /// </summary>
        /// <value>
        /// The methods in the struct.
        /// </value>
        public IEnumerable<MethodContext> Methods { get; }

        /// <summary>
        /// Gets the properties in the struct.
        /// </summary>
        /// <value>
        /// The properties in the struct.
        /// </value>
        public IEnumerable<PropertyContext> Properties { get; }

        /// <summary>
        /// Gets the fields in the struct.
        /// </summary>
        /// <value>
        /// The fields in the struct.
        /// </value>
        public IEnumerable<VariableContext> Fields { get; }

        /// <summary>
        /// Gets the symbol for the struct.
        /// </summary>
        /// <value>
        /// The symbol for the struct.
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
        /// Gets the generic type arguments, if any.
        /// </summary>
        /// <value>
        /// The generic type arguments, if any.
        /// </value>
        public IEnumerable<TypeContext> TypeArguments { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StructContext"/> class.
        /// </summary>
        /// <param name="structDeclaration">The structure declaration.</param>
        /// <param name="semanticModel">The semantic model.</param>
        /// <exception cref="System.ArgumentNullException">structDeclaration</exception>
        /// <exception cref="System.ArgumentException">semanticModel</exception>
        public StructContext(StructDeclarationSyntax structDeclaration, SemanticModel semanticModel)
        {
            Declaration = structDeclaration ?? throw new ArgumentNullException(nameof(structDeclaration));
            SemanticModel = semanticModel ?? throw new ArgumentException(nameof(semanticModel));

            Symbol = semanticModel.GetDeclaredSymbol(structDeclaration);
            Properties = this.GetDescendantPropertyContexts(semanticModel).ToList();
            Methods = this.GetDescendantMethodContexts(semanticModel).ToList();
            Fields = this.GetDescendantFieldContexts(semanticModel).ToList();
            Constructors = this.GetDescendantConstructorContexts(semanticModel).ToList();

            if (Symbol is INamedTypeSymbol named)
            {
                if (named.IsGenericType)
                {
                    TypeArguments = named
                        .TypeArguments
                        .Select(x => new TypeContext(x, semanticModel))
                        .ToList();
                }
                else
                {
                    TypeArguments = new TypeContext[] { };
                }
            }
            else
                Debugger.Break();
        }

        /// <summary>
        /// Gets the name of the class.
        /// </summary>
        public string Name
            => Symbol.Name;

        /// <summary>
        /// Gets a value indicating whether this struct has the partial modifier.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has the partial modifier; otherwise, <c>false</c>.
        /// </value>
        public bool IsPartial
            => Declaration.IsPartial();

        /// <summary>
        /// Gets a value indicating whether struct has the static modifier.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance struct has the static modifier; otherwise, <c>false</c>.
        /// </value>
        public bool IsStatic
            => Symbol.IsStatic;

        /// <summary>
        /// Gets the full name of the type.
        /// </summary>
        public string FullName
            => NameBuilder.GetFullName(TypeArguments, Name, IsGeneric, isValueTuple: false, isArray: false, includeGenericTypeParamNames: true);

        /// <summary>
        /// Gets the full name without the names of the generic type arguments.
        /// </summary>
        /// <value>
        /// The full name without the names of the generic type arguments.
        /// </value>
        public string FullNameWithoutGenericNames
            => NameBuilder.GetFullName(TypeArguments, Name, IsGeneric, isValueTuple: false, isArray: false, includeGenericTypeParamNames: false);

        /// <summary>
        /// Gets a value indicating whether this instance is generic.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is generic; otherwise, <c>false</c>.
        /// </value>
        public bool IsGeneric
            => Symbol.IsGenericType;

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var stat = Symbol.IsStatic ? "static " : string.Empty;

            //todo: add generic type parameters
            return $"{stat}struct {FullName}";
        }

        /// <summary>
        /// Gets the key that identifies an object.
        /// </summary>
        /// <value>
        /// The object's key.
        /// </value>
        public string Key
            => ToString();

        public string DebuggerDisplay
            => ToString();
    }
}