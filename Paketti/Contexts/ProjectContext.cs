using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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

            var compilation = this.NewCompilation;
            this.Documents = Project
                .Documents
                .Select(x => new DocumentContext(this, x))
                .ToList();
        }

        /// <summary>
        /// Gets the syntax trees of all the documents in the project.
        /// </summary>
        /// <value>
        /// The syntax trees of all the documents in the project.
        /// </value>
        public IEnumerable<SyntaxTree> DocumentSyntaxTrees
            => Project.Documents.Select(x => x.GetSyntaxTreeAsync().Result);

        /// <summary>
        /// Gets a new compilation using all the documents in this project.
        /// </summary>
        /// <value>
        /// The new compilation.
        /// </value>
        public CSharpCompilation NewCompilation
            => CSharpCompilation.Create(Project.AssemblyName, DocumentSyntaxTrees);

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
    }
}