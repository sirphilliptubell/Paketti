using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Paketti.Primitives;

namespace Paketti.Contexts
{
    /// <summary>
    /// The contextual information for a Project.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class ProjectContext
    {
        /// <summary>
        /// Gets the documents in the project.
        /// </summary>
        /// <value>
        /// The documents in the project.
        /// </value>
        public IEnumerable<DocumentContext> Documents { get; }

        /// <summary>
        /// Gets the project.
        /// </summary>
        /// <value>
        /// The project.
        /// </value>
        public Project Project { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectContext"/> class.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <exception cref="System.ArgumentNullException">project</exception>
        private ProjectContext(Project project)
        {
            this.Project = project ?? throw new ArgumentNullException(nameof(project));

            //this may throw an exception if the project doesn't compile
            this.Compilation = CSharpCompilation.Create(Project.AssemblyName, DocumentSyntaxTrees, Project.MetadataReferences);

            this.Documents = Project
                .Documents
                .Select(x => new DocumentContext(this, x))
                .ToList();
        }

        /// <summary>
        /// Gets the compilation.
        /// </summary>
        public CSharpCompilation Compilation { get; }

        /// <summary>
        /// Gets the syntax trees of all the documents in the project.
        /// </summary>
        /// <value>
        /// The syntax trees of all the documents in the project.
        /// </value>
        public IEnumerable<SyntaxTree> DocumentSyntaxTrees
            => Project.Documents.Select(x => x.GetSyntaxTreeAsync().Result);

        /// <summary>
        /// Gets the name of the assembly for the project.
        /// </summary>
        public AssemblyName AssemblyName
            => AssemblyName.Create(Project.AssemblyName).Value;

        /// <summary>
        /// Tries to create a ProjectContext using the specified project.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <returns></returns>
        public static Result<ProjectContext> Create(Project project)
        {
            try
            {
                //May throw if the project doesn't compile
                return Result.Ok(new ProjectContext(project));
            }
            catch (Exception ex)
            {
                return Result.Fail<ProjectContext>(ex);
            }
        }

        /// <summary>
        /// Gets the classes and structs in all the documents.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ITypeDeclarationContext> GetClassesAndStructs()
            => Documents.SelectMany(x => x.GetClassesAndStructs());

        /// <summary>
        /// Gets the extension methods in all the documents.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<MethodContext> GetExtensionMethods()
            => Documents
            .SelectMany(x => x.GetExtensionMethods());

        /// <summary>
        /// Gets the type members (eg: methods/properties/etc...)
        /// Does not include extension methods.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ITypeMemberContext> GetTypeMembersExcludingExtensions()
            => Documents
            .SelectMany(x => x.GetTypeMembersExcludingExtensions());

        /// <summary>
        /// Gets the types that can contain members.
        /// </summary>
        /// <returns></returns>
        public (IEnumerable<ClassContext> classes, IEnumerable<StructContext> structs) GetMemberContainers()
            //todo: add interfaces
            => (Documents.SelectMany(x => x.Classes), Documents.SelectMany(x => x.Structs));

        /// <summary>
        /// Gets a document context with the specified Id.
        /// </summary>
        /// <param name="documentId">The document identifier.</param>
        /// <returns></returns>
        public DocumentContext GetDocumentContext(DocumentId documentId)
            => Documents
            .Where(x => x.Document.Id == documentId)
            .Single();

        /// <summary>
        /// Gets the name of the project.
        /// </summary>
        /// <value>
        /// The name of the project.
        /// </value>
        public string Name
            => Project.Name;

        private string DebuggerDisplay
            => $"{Name} (Assembly: {Project.AssemblyName})";

        ///// <summary>
        ///// Gets the unused (or duplicate) using directives.
        ///// </summary>
        ///// <param name="root">The document root.</param>
        ///// <returns></returns>
        //internal Dictionary<DocumentId, IEnumerable<UsingDirectiveSyntax>> GetUnusedUsingDirectives()
        //{
        //    var result = new HashSet<SyntaxNode>();

        //    var diagnostics = root
        //        .GetDiagnostics()
        //        .Where(d => d.Id == DiagnosticId.DUPLICATE_USING_DIRECTIVE || d.Id == DiagnosticId.UNNECESSARY_USING_DIRECTIVE);

        //    foreach (var diagnostic in diagnostics)
        //    {
        //        if (root.FindNode(diagnostic.Location.SourceSpan) is UsingDirectiveSyntax syntax)
        //        {
        //            result.Add(syntax);
        //        }
        //    }

        //    return result;
        //}
    }
}