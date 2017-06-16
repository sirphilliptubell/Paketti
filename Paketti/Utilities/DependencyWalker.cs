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
        private static readonly IEnumerable<TypeContext> EMPTY_RESULT = new TypeContext[] { };
        private readonly ILog _log;
        private readonly Dictionary<ITypeDependent, List<TypeContext>> _cache = new Dictionary<ITypeDependent, List<TypeContext>>();

        /// <summary>
        /// Gets the project the walker is assigned to.
        /// </summary>
        /// <value>
        /// The project context.
        /// </value>
        public ProjectContext ProjectContext { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyWalker"/> class.
        /// </summary>
        /// <param name="projectContext">The project context.</param>
        /// <param name="log">The log.</param>
        /// <exception cref="System.ArgumentNullException">projectContext</exception>
        /// <exception cref="System.ArgumentException">log</exception>
        public DependencyWalker(ProjectContext projectContext, ILog log)
        {
            ProjectContext = projectContext ?? throw new ArgumentNullException(nameof(projectContext));
            _log = log ?? throw new ArgumentException(nameof(log));
        }

        /// <summary>
        /// Gets the types a symbol is dependent on.
        /// Returns the cached value when possible.
        /// Ensures the <paramref name="visited"/> parameter is not null before <paramref name="calculateDependencies"/> is called.
        /// </summary>
        /// <param name="symbol">The symbol to get the dependencies for.</param>
        /// <param name="calculateDependencies">The method that calculates the dependencies.</param>
        /// <param name="visited">The symbols already visited.</param>
        /// <returns></returns>
        private IEnumerable<TypeContext> GetTypeDependencies(
            ITypeDependent symbol,
            Func<IEnumerable<TypeContext>> calculateDependencies,
            ref HashSet<ITypeDependent> visited)
        {
            //check the cache first
            if (_cache.ContainsKey(symbol))
                return _cache[symbol];

            //prevent infinite loops due to circular dependencies
            //If A requires B requires A, and the user has requested the types for A
            //the second time A is requested, it'll be in the visited list and skipped
            //when the call stack finishes getting the dependencies for B and returns to the first A
            //then A will then be finally computed and cached

            visited = visited ?? new HashSet<ITypeDependent>();
            if (visited.Contains(symbol))
                return EMPTY_RESULT;
            else
                visited.Add(symbol);

            //get the dependencies
            //note: dur to the "ref", the "visited" param within the calling method will
            //be non-null for when the lambda provided for calculateDependencies() is called
            List<TypeContext> result;
            using (_log.LogStep($"Walking dependencies of {symbol.ToString()}"))
            {
                result = calculateDependencies().Distinct().ToList();
            }
            //cache
            _cache[symbol] = result;

            return result;
        }

        /// <summary>
        /// Gets the type dependencies for a delegate.
        /// </summary>
        /// <param name="del"></param>
        /// <param name="visited">The set of type dependent symbols which have already been visited.</param>
        /// <returns></returns>
        public IEnumerable<TypeContext> GetTypeDependencies(DelegateContext del, HashSet<ITypeDependent> visited = null)
            => GetTypeDependencies(del, () =>
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
            }, ref visited);

        /// <summary>
        /// Gets the type dependencies for a type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="visited">The set of type dependent symbols which have already been visited.</param>
        /// <returns></returns>
        public IEnumerable<TypeContext> GetTypeDependencies(TypeContext type, HashSet<ITypeDependent> visited = null)
            => GetTypeDependencies(type, () =>
            {
                var result =
                    //The generic type arguments of the type, eg: the T1, T1 in Tuple<T1, T2>
                    type.TypeArguments
                    //The type arguments of the type arguments and all the way down, eg: the U in Tuple<T1, List<U>>
                    .Union(type.TypeArguments.SelectMany(x => GetTypeDependencies(x, visited)))
                    .Distinct();

                return result;
            }, ref visited);

        /// <summary>
        /// Gets the type dependencies for a variable/field.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <param name="visited">The set of type dependent symbols which have already been visited.</param>
        /// <returns></returns>
        public IEnumerable<TypeContext> GetTypeDependencies(VariableContext variable, HashSet<ITypeDependent> visited = null)
            => GetTypeDependencies(variable, () =>
            {
                var typeContext = new TypeContext(variable.Symbol.Type, variable.SemanticModel);

                var result =
                    //The type of the variable itself
                    new TypeContext[] { typeContext }
                    //The types that the variable type depends on (it could be a generic type)
                    .Union(GetTypeDependencies(typeContext, visited))
                    .Distinct();

                return result;
            }, ref visited);

        /// <summary>
        /// Gets the type dependencies for a constructor.
        /// </summary>
        /// <param name="ctr"></param>
        /// <param name="visited">The set of type dependent symbols which have already been visited.</param>
        /// <returns></returns>
        public IEnumerable<TypeContext> GetTypeDependencies(ConstructorContext ctr, HashSet<ITypeDependent> visited = null)
            => GetTypeDependencies(ctr, () =>
            {
                var result =
                    //The type of the constructor's parameters
                    ctr.Symbol.Parameters.Select(x => new TypeContext(x.Type, ctr.SemanticModel))
                    //The types directly mentioned in the body
                    .Union(
                        GetBodyDeclaredTypes(ctr.Declaration, ctr.SemanticModel, visited)
                        .Select(x => new TypeContext(x, ctr.SemanticModel))
                    )
                    //The local methods and properties mentioned in the body (which themselves may have dependencies)
                    .Union(GetBodyInvocationSymbols(ctr.Declaration, ctr.SemanticModel, visited))
                    .Distinct();

                return result;
            }, ref visited);

        /// <summary>
        /// Gets the type dependencies for a method.
        /// </summary>
        /// <param name="method"></param>
        /// <param name="visited">The set of type dependent symbols which have already been visited.</param>
        /// <returns></returns>
        public IEnumerable<TypeContext> GetTypeDependencies(MethodContext method, HashSet<ITypeDependent> visited = null)
            => GetTypeDependencies(method, () =>
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
                        GetBodyDeclaredTypes(method.Declaration, method.SemanticModel, visited)
                        .Select(x => new TypeContext(x, method.SemanticModel))
                    )
                    //The local methods and properties mentioned in the body (which themselves may have dependencies)
                    .Union(GetBodyInvocationSymbols(method.Declaration, method.SemanticModel, visited))
                    .Distinct();

                return result;
            }, ref visited);

        /// <summary>
        /// Gets the type dependencies for a property.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="visited">The set of type dependent symbols which have already been visited.</param>
        /// <returns></returns>
        public IEnumerable<TypeContext> GetTypeDependencies(PropertyContext property, HashSet<ITypeDependent> visited = null)
            => GetTypeDependencies(property, () =>
            {
                var result =
                    //The property's return type
                    new TypeContext[] { new TypeContext(property.Declaration.Type, property.SemanticModel) }
                    //The types directly mentioned in the body
                    .Union(
                        GetBodyDeclaredTypes(property.Declaration, property.SemanticModel, visited)
                        .Select(x => new TypeContext(x, property.SemanticModel))
                    )
                    //The local methods and properties mentioned in the body (which themselves may have dependencies)
                    .Union(GetBodyInvocationSymbols(property.Declaration, property.SemanticModel, visited))
                    .Distinct();

                return result;
            }, ref visited);

        /// <summary>
        /// Gets the type dependencies for a struct.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="visited">The set of type dependent symbols which have already been visited.</param>
        /// <returns></returns>
        public IEnumerable<TypeContext> GetTypeDependencies(StructContext str, HashSet<ITypeDependent> visited = null)
            => GetTypeDependencies(str, () =>
            {
                var result = str.Fields.SelectMany(x => GetTypeDependencies(x, visited))
                    .Union(str.Constructors.SelectMany(x => GetTypeDependencies(x, visited)))
                    .Union(str.Properties.SelectMany(x => GetTypeDependencies(x, visited)))
                    .Union(str.Methods.SelectMany(x => GetTypeDependencies(x, visited)))
                    .Distinct();

                return result;
            }, ref visited);

        /// <summary>
        /// Gets the type dependencies for a class.
        /// </summary>
        /// <param name="cls">The class.</param>
        /// <param name="visited">The set of type dependent symbols which have already been visited.</param>
        /// <returns></returns>
        public IEnumerable<TypeContext> GetTypeDependencies(ClassContext cls, HashSet<ITypeDependent> visited = null)
            => GetTypeDependencies(cls, () =>
            {
                var result = cls.Fields.SelectMany(x => GetTypeDependencies(x, visited))
                    .Union(cls.Constructors.SelectMany(x => GetTypeDependencies(x, visited)))
                    .Union(cls.Properties.SelectMany(x => GetTypeDependencies(x, visited)))
                    .Union(cls.Methods.SelectMany(x => GetTypeDependencies(x, visited)))
                    .Distinct();

                return result;
            }, ref visited);

        /// <summary>
        /// Finds the context dependencies.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="visited">The set of type dependent symbols which have already been visited.</param>
        /// <returns></returns>
        private IEnumerable<TypeContext> FindContextDependencies(SyntaxNode node, HashSet<ITypeDependent> visited)
        {
            if (node == null) throw new ArgumentException(nameof(node));
            if (visited == null) throw new ArgumentException(nameof(visited));

            foreach (var doc in ProjectContext.Documents)
            {
                foreach (var cls in doc.Classes)
                {
                    foreach (var ctr in cls.Constructors)
                        if (ctr.Declaration == node)
                            return GetTypeDependencies(ctr, visited);

                    foreach (var fld in cls.Fields)
                        if (fld.Declaration == node)
                            return GetTypeDependencies(fld, visited);

                    foreach (var mth in cls.Methods)
                        if (mth.Declaration == node)
                            return GetTypeDependencies(mth, visited);

                    foreach (var prp in cls.Properties)
                        if (prp.Declaration == node)
                            return GetTypeDependencies(prp, visited);
                }

                foreach (var str in doc.Structs)
                {
                    foreach (var ctr in str.Constructors)
                        if (ctr.Declaration == node)
                            return GetTypeDependencies(ctr, visited);

                    foreach (var fld in str.Fields)
                        if (fld.Declaration == node)
                            return GetTypeDependencies(fld, visited);

                    foreach (var mth in str.Methods)
                        if (mth.Declaration == node)
                            return GetTypeDependencies(mth, visited);

                    foreach (var prp in str.Properties)
                        if (prp.Declaration == node)
                            return GetTypeDependencies(prp, visited);
                }

                foreach (var del in doc.Delegates)
                {
                    if (del.Declaration == node)
                        return GetTypeDependencies(del, visited);
                }
            }

            return EMPTY_RESULT;
        }

        /// <summary>
        /// Gets the types declared within a body.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="semanticModel">The semantic model.</param>
        /// <param name="visited">The set of type dependent symbols which have already been visited.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">
        /// node
        /// or
        /// semanticModel
        /// </exception>
        private IEnumerable<ITypeSymbol> GetBodyDeclaredTypes(SyntaxNode node, SemanticModel semanticModel, HashSet<ITypeDependent> visited)
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
        /// <param name="visited">The set of type dependent symbols which have already been visited.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">
        /// node
        /// or
        /// semanticModel
        /// </exception>
        private IEnumerable<TypeContext> GetBodyInvocationSymbols(SyntaxNode node, SemanticModel semanticModel, HashSet<ITypeDependent> visited)
        {
            if (node == null) throw new ArgumentException(nameof(node));
            if (semanticModel == null) throw new ArgumentException(nameof(semanticModel));
            if (visited == null) throw new ArgumentException(nameof(visited));

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
                .SelectMany(x => FindContextDependencies(x, visited))
                .Distinct();

            return r;
        }
    }
}