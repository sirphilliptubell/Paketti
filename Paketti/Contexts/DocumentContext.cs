using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Paketti.Contexts
{
    /// <summary>
    /// The contextual information for a Document.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class DocumentContext
    {
        /// <summary>
        /// Gets the document.
        /// </summary>
        /// <value>
        /// The document.
        /// </value>
        public Document Document { get; }

        /// <summary>
        /// Gets the classes in the document (including nested ones).
        /// </summary>
        /// <value>
        /// The classes in the document (including nested ones).
        /// </value>
        public IEnumerable<ClassContext> Classes { get; }

        /// <summary>
        /// Gets the delegates in the document (including nested ones).
        /// </summary>
        /// <value>
        /// The delegates in the document (including nested ones).
        /// </value>
        public IEnumerable<DelegateContext> Delegates { get; }

        /// <summary>
        /// Gets the structs in the document (including nested ones).
        /// </summary>
        /// <value>
        /// The structs in the document (including nested ones).
        /// </value>
        public IEnumerable<StructContext> Structs { get; }

        /// <summary>
        /// Gets the project context for the document.
        /// </summary>
        /// <value>
        /// The project context for the document.
        /// </value>
        public ProjectContext ProjectContext { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentContext"/> class.
        /// </summary>
        /// <param name="projectContext">The project context.</param>
        /// <param name="document">The document.</param>
        /// <exception cref="System.ArgumentException">
        /// projectContext
        /// or
        /// document
        /// </exception>
        public DocumentContext(ProjectContext projectContext, Document document)
        {
            ProjectContext = projectContext ?? throw new ArgumentException(nameof(projectContext));
            Document = document ?? throw new ArgumentException(nameof(document));

            //Todo, we should probably limit these classes, structs, and delegates to top-level (not nested) items
            //Use separate properties for nested entries.
            Classes = SyntaxRoot
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Select(x => new ClassContext(x, SemanticModel))
                .ToList();

            Structs = SyntaxRoot
                .DescendantNodes()
                .OfType<StructDeclarationSyntax>()
                .Select(x => new StructContext(x, SemanticModel))
                .ToList();

            Delegates = SyntaxRoot
                .DescendantNodes()
                .OfType<DelegateDeclarationSyntax>()
                .Select(x => new DelegateContext(x, SemanticModel))
                .ToList();
        }

        /// <summary>
        /// Gets the document's name.
        /// </summary>
        /// <value>
        /// The document's name.
        /// </value>
        public string Name
            => Document.Name;

        /// <summary>
        /// Gets the project of the document.
        /// </summary>
        /// <value>
        /// The project of the document.
        /// </value>
        public Project Project
            => ProjectContext.Project;

        /// <summary>
        /// Gets the semantic model.
        /// </summary>
        /// <value>
        /// The semantic model.
        /// </value>
        public SemanticModel SemanticModel
            => Document.GetSemanticModelAsync().Result;

        /// <summary>
        /// Gets the syntax root.
        /// </summary>
        /// <value>
        /// The syntax root.
        /// </value>
        public SyntaxNode SyntaxRoot
            => Document.GetSyntaxRootAsync().Result;

        /// <summary>
        /// Gets the syntax tree.
        /// </summary>
        /// <value>
        /// The syntax tree.
        /// </value>
        public SyntaxTree SyntaxTree
            => Document.GetSyntaxTreeAsync().Result;

        /// <summary>
        /// Gets the classes and structs in the document (including nested ones).
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IClassOrStruct> GetClassesAndStructs()
            => ((IEnumerable<IClassOrStruct>)Classes)
            .Union(Structs);

        /// <summary>
        /// Gets all extension methods in the document (including nested ones).
        /// </summary>
        /// <returns></returns>
        public IEnumerable<MethodContext> GetExtensionMethods()
            => Classes
            .Where(x => x.IsStatic)
            .SelectMany(x => x.Methods)
            .Where(x => x.IsExtensionMethod);

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
            => Name;

        private string DebuggerDisplay
            => $"classes:{Classes.Count()}, structs:{Structs.Count()}, delegates:{Delegates.Count()} {Document.FilePath}";
    }
}