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
    /// The contextual information for a ClassDeclaration.
    /// </summary>
    /// <seealso cref="Paketti.Contexts.ITypeDeclarationContext" />
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class ClassContext :
        ITypeDependent,
        ITypeDeclarationContext
    {
        /// <summary>
        /// Gets the class declaration.
        /// </summary>
        /// <value>
        /// The class declaration.
        /// </value>
        public ClassDeclarationSyntax Declaration { get; }

        /// <summary>
        /// Gets the type declaration syntax.
        /// </summary>
        /// <value>
        /// The declaration.
        /// </value>
        TypeDeclarationSyntax ITypeDeclarationContext.Declaration => this.Declaration;

        /// <summary>
        /// Gets the constructors.
        /// </summary>
        /// <value>
        /// The constructors.
        /// </value>
        public IEnumerable<ConstructorContext> Constructors { get; }

        /// <summary>
        /// Gets the methods.
        /// </summary>
        /// <value>
        /// The methods.
        /// </value>
        public IEnumerable<MethodContext> Methods { get; }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <value>
        /// The properties.
        /// </value>
        public IEnumerable<PropertyContext> Properties { get; }

        /// <summary>
        /// Gets the fields.
        /// </summary>
        /// <value>
        /// The fields.
        /// </value>
        public IEnumerable<VariableContext> Fields { get; }

        /// <summary>
        /// Gets the symbol of the class.
        /// </summary>
        /// <value>
        /// The symbol.
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
        /// Initializes a new instance of the <see cref="ClassContext"/> class.
        /// </summary>
        /// <param name="classDeclaration">The class declaration.</param>
        /// <param name="compilation">The compilation.</param>
        /// <param name="semanticModel">The semantic model.</param>
        /// <exception cref="System.ArgumentNullException">classDeclaration</exception>
        /// <exception cref="System.ArgumentException">
        /// compilation
        /// or
        /// semanticModel
        /// </exception>
        public ClassContext(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel)
        {
            Declaration = classDeclaration ?? throw new ArgumentNullException(nameof(classDeclaration));
            SemanticModel = semanticModel ?? throw new ArgumentException(nameof(semanticModel));

            Symbol = semanticModel.GetDeclaredSymbol(classDeclaration);
            Properties = this.GetDescendantPropertyContexts(semanticModel).ToList();
            Methods = this.GetDescendantMethodContexts(semanticModel).ToList();
            Fields = this.GetDescendantFieldContexts(semanticModel).ToList();
            Constructors = this.GetDescendantConstructorContexts(semanticModel).ToList();
        }

        /// <summary>
        /// Gets a value indicating whether this class has the partial modifier.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has the partial modifier; otherwise, <c>false</c>.
        /// </value>
        public bool IsPartial
            => Declaration.IsPartial();

        /// <summary>
        /// Gets a value indicating whether this instance has the static modifier.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has the static modifier; otherwise, <c>false</c>.
        /// </value>
        public bool IsStatic
            => Symbol.IsStatic;

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var stat = IsStatic ? "static " : string.Empty;

            //todo: add generic type parameters
            return $"{stat}class {Symbol.ContainingAssembly.Name}.{Symbol.Name}<TODO>";
        }

        /// <summary>
        /// Gets the key that identifies an object.
        /// </summary>
        /// <value>
        /// The object's key.
        /// </value>
        public string Key
            => ToString();

        private string DebuggerDisplay
            => ToString();
    }
}