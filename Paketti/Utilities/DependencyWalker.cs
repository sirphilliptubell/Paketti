using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Paketti.Contexts;
using Paketti.Extensions;
using Paketti.Logging;

namespace Paketti.Utilities
{
    /// <summary>
    /// Gets the dependencies for various Syntax Contexts.
    /// </summary>
    /// <seealso cref="Paketti.Utilities.IDependencyWalker" />
    public class DependencyWalker :
        IDependencyWalker
    {
        private readonly ILog _log;
        private readonly ProjectContext _project;

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyWalker"/> class.
        /// </summary>
        /// <param name="projectContext">The project context.</param>
        /// <param name="log">The log.</param>
        /// <exception cref="System.ArgumentNullException">projectContext</exception>
        /// <exception cref="System.ArgumentException">log</exception>
        public DependencyWalker(ProjectContext projectContext, ILog log)
        {
            _project = projectContext ?? throw new ArgumentNullException(nameof(projectContext));
            _log = log ?? throw new ArgumentException(nameof(log));
        }

        /// <summary>
        /// Gets the type dependencies for a delegate.
        /// </summary>
        /// <param name="del"></param>
        /// <returns></returns>
        public IEnumerable<TypeContext> GetTypeDependencies(DelegateContext del)
        {
            var result =
                //The return type of the delegate
                new TypeContext[] { new TypeContext(del.Symbol.DelegateInvokeMethod.ReturnType, del.SemanticModel) }
                //The type of the delegate's parameters
                .Union(
                    del.Symbol.DelegateInvokeMethod.Parameters
                    .Select(x => new TypeContext(x.Type, del.SemanticModel))
                )
                .Distinct();

            return result;
        }

        /// <summary>
        /// Gets the type dependencies for a type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IEnumerable<TypeContext> GetTypeDependencies(TypeContext type)
        {
            var result =
                //The generic type arguments of the type, eg: the T1, T1 in Tuple<T1, T2>
                type.TypeArguments
                //The type arguments of the type arguments and all the way down, eg: the U in Tuple<T1, List<U>>
                .Union(type.TypeArguments.SelectMany(x => GetTypeDependencies(x)))
                .Distinct();

            return result;
        }

        /// <summary>
        /// Gets the type dependencies for a variable/field.
        /// </summary>
        /// <param name="variable"></param>
        /// <returns></returns>
        public IEnumerable<TypeContext> GetTypeDependencies(VariableContext variable)
        {
            var typeContext = new TypeContext(variable.Symbol.Type, variable.SemanticModel);

            var result =
                //The type of the variable itself
                new TypeContext[] { typeContext }
                //The types that the variable type depends on (it could be a generic type)
                .Union(GetTypeDependencies(typeContext))
                .Distinct();

            return result;
        }

        /// <summary>
        /// Gets the type dependencies for a constructor.
        /// </summary>
        /// <param name="ctr"></param>
        /// <returns></returns>
        public IEnumerable<TypeContext> GetTypeDependencies(ConstructorContext ctr)
        {
            var result =
                //The type of the constructor's parameters
                ctr.Symbol.Parameters.Select(x => new TypeContext(x.Type, ctr.SemanticModel))
                //The types directly mentioned in the body
                .Union(
                    GetBodyDeclaredTypes(ctr.Declaration, ctr.SemanticModel)
                    .Select(x => new TypeContext(x, ctr.SemanticModel))
                )
                //The local methods and properties mentioned in the body (which themselves may have dependencies)
                .Union(GetBodyInvocationSymbols(ctr.Declaration, ctr.SemanticModel))
                .Distinct();

            return result;
        }

        /// <summary>
        /// Gets the type dependencies for a method.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public IEnumerable<TypeContext> GetTypeDependencies(MethodContext method)
        {
            using (_log.LogStep($"{nameof(DependencyWalker)}.{nameof(GetTypeDependencies)}.Method"))
            {
                var result =
                    //The method's return type
                    new TypeContext[] { new TypeContext(method.Declaration.ReturnType, method.SemanticModel) }
                    //The type of the method's parameters
                    .Union(
                        method
                        .Symbol
                        .Parameters
                        .Select(x => new TypeContext(x.Type, method.SemanticModel))
                    )
                    //The types directly mentioned in the body
                    .Union(
                        GetBodyDeclaredTypes(method.Declaration, method.SemanticModel)
                        .Select(x => new TypeContext(x, method.SemanticModel))
                    )
                    //The local methods and properties mentioned in the body (which themselves may have dependencies)
                    .Union(GetBodyInvocationSymbols(method.Declaration, method.SemanticModel))
                    .Distinct();

                return result;
            }
        }

        /// <summary>
        /// Gets the type dependencies for a property.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public IEnumerable<TypeContext> GetTypeDependencies(PropertyContext property)
        {
            var result =
                //The property's return type
                new TypeContext[] { new TypeContext(property.Declaration.Type, property.SemanticModel) }
                //The types directly mentioned in the body
                .Union(
                    GetBodyDeclaredTypes(property.Declaration, property.SemanticModel)
                    .Select(x => new TypeContext(x, property.SemanticModel))
                )
                //The local methods and properties mentioned in the body (which themselves may have dependencies)
                .Union(GetBodyInvocationSymbols(property.Declaration, property.SemanticModel))
                .Distinct();

            return result;
        }

        /// <summary>
        /// Gets the type dependencies for a struct.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public IEnumerable<TypeContext> GetTypeDependencies(StructContext str)
        {
            var result = str.Fields.SelectMany(x => GetTypeDependencies(x))
                .Union(str.Constructors.SelectMany(x => GetTypeDependencies(x)))
                .Union(str.Properties.SelectMany(x => GetTypeDependencies(x)))
                .Union(str.Methods.SelectMany(x => GetTypeDependencies(x)))
                .Distinct();

            return result;
        }

        /// <summary>
        /// Gets the type dependencies for a class.
        /// </summary>
        /// <param name="cls">The class.</param>
        /// <returns></returns>
        public IEnumerable<TypeContext> GetTypeDependencies(ClassContext cls)
        {
            var result = cls.Fields.SelectMany(x => GetTypeDependencies(x))
                .Union(cls.Constructors.SelectMany(x => GetTypeDependencies(x)))
                .Union(cls.Properties.SelectMany(x => GetTypeDependencies(x)))
                .Union(cls.Methods.SelectMany(x => GetTypeDependencies(x)))
                .Distinct();

            return result;
        }

        /// <summary>
        /// Finds the context dependencies.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        private IEnumerable<TypeContext> FindContextDependencies(SyntaxNode node)
        {
            foreach (var doc in _project.Documents)
            {
                foreach (var cls in doc.Classes)
                {
                    foreach (var ctr in cls.Constructors)
                        if (ctr.Declaration == node)
                            return GetTypeDependencies(ctr);

                    foreach (var fld in cls.Fields)
                        if (fld.Declaration == node)
                            return GetTypeDependencies(fld);

                    foreach (var mth in cls.Methods)
                        if (mth.Declaration == node)
                            return GetTypeDependencies(mth);

                    foreach (var prp in cls.Properties)
                        if (prp.Declaration == node)
                            return GetTypeDependencies(prp);
                }

                foreach (var str in doc.Structs)
                {
                    foreach (var ctr in str.Constructors)
                        if (ctr.Declaration == node)
                            return GetTypeDependencies(ctr);

                    foreach (var fld in str.Fields)
                        if (fld.Declaration == node)
                            return GetTypeDependencies(fld);

                    foreach (var mth in str.Methods)
                        if (mth.Declaration == node)
                            return GetTypeDependencies(mth);

                    foreach (var prp in str.Properties)
                        if (prp.Declaration == node)
                            return GetTypeDependencies(prp);
                }

                foreach (var del in doc.Delegates)
                {
                    if (del.Declaration == node)
                        return GetTypeDependencies(del);
                }
            }

            //investigate
            //Debugger.Break();

            return new TypeContext[] { };
        }

        /// <summary>
        /// Gets the types declared within a body.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="semanticModel">The semantic model.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">
        /// node
        /// or
        /// semanticModel
        /// </exception>
        private IEnumerable<ITypeSymbol> GetBodyDeclaredTypes(SyntaxNode node, SemanticModel semanticModel)
        {
            if (node == null) throw new ArgumentException(nameof(node));
            if (semanticModel == null) throw new ArgumentException(nameof(semanticModel));

            var bodyBlock
                = node
                .DescendantNodes()
                .OfType<BlockSyntax>()
                .FirstOrDefault();

            var expressionBlock
                = node
                .DescendantNodes()
                .OfType<ArrowExpressionClauseSyntax>()
                .FirstOrDefault();

            var getBlock
                = node
                .DescendantNodes()
                .OfType<AccessorDeclarationSyntax>()
                .Where(x => x.Kind() == SyntaxKind.GetAccessorDeclaration)
                .FirstOrDefault();

            var setBlock
                = node
                .DescendantNodes()
                .OfType<AccessorDeclarationSyntax>()
                .Where(x => x.Kind() == SyntaxKind.SetAccessorDeclaration)
                .FirstOrDefault();

            var result = new SyntaxNode[] { bodyBlock, expressionBlock, getBlock, setBlock }
                .Where(x => x != null)
                .SelectMany(x => x.DescendantNodes())
                .Select(x => semanticModel.GetTypeInfo(x).Type)
                .Where(x => x != null && x.Kind != SymbolKind.ErrorType); //ErrorType kind is not allowed by TypeContext

            return result;
        }

        /// <summary>
        /// Gets the types used within an invocation within a body.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="semanticModel">The semantic model.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">
        /// node
        /// or
        /// semanticModel
        /// </exception>
        private IEnumerable<TypeContext> GetBodyInvocationSymbols(SyntaxNode node, SemanticModel semanticModel)
        {
            if (node == null) throw new ArgumentException(nameof(node));
            if (semanticModel == null) throw new ArgumentException(nameof(semanticModel));

            // Find the types which are hidden within local properties and/or functions.

            /* example situation, SomeDependencyType is another type which is required by SomeFunction, but it's never used within SomeFunction.
             * SomeFunction actually depends on { Type, string, SomeDependencyType }
             *
             * string SomeProperty
             *      => SomeDependencyType.ToString()
             *
             * string SomeFunction()
             *      => SomeProperty;
             */
            var r = node
                .DescendantNodes()
                .Select(x => semanticModel.GetSymbolInfo(x).Symbol)
                .OnlyValues()
                .SelectMany(x => x.DeclaringSyntaxReferences)
                .Select(x => x.GetSyntax())
                .SelectMany(x => FindContextDependencies(x))
                .Distinct();

            return r;
        }
    }
}