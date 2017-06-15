using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Paketti.Contexts;
using Paketti.Extensions;

namespace Paketti.Rewriters
{
    /// <summary>
    /// Wrapper for handling document/project changes.
    /// All methods return a new document with changes applied.
    /// </summary>
    internal class DocumentState
    {
        private readonly Document _doc;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentState"/> class.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <exception cref="System.ArgumentNullException">document</exception>
        public DocumentState(Document document)
        {
            _doc = document ?? throw new ArgumentNullException(nameof(document));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentState"/> class.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="documentId">The document id.</param>
        public DocumentState(Project project, DocumentId documentId)
            : this(project?.GetDocument(documentId))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentState"/> class.
        /// </summary>
        /// <param name="documentContext">The document context.</param>
        public DocumentState(DocumentContext documentContext)
            : this(documentContext?.Document)
        { }

        /// <summary>
        /// Gets the document.
        /// </summary>
        /// <value>
        /// The document.
        /// </value>
        public Document Document
            => _doc;

        /// <summary>
        /// Gets a new instance of DocumentState with the specified changed document.
        /// </summary>
        /// <param name="alter">The function that alters the document.</param>
        /// <returns></returns>
        public DocumentState AlterDocument(Func<Document, Document> alter)
            => new DocumentState(alter(_doc));

        /// <summary>
        /// Gets a new instance of DocumentState with the specified changed document.
        /// </summary>
        /// <param name="alter">The function that alters the document.</param>
        /// <returns></returns>
        public Result<DocumentState> AlterDocument(Func<Document, Result<Document>> alter)
            => alter(_doc)
            .ToTypedResult(doc => new DocumentState(doc));

        /// <summary>
        /// Gets a new instance of DocumentState with the specified changed document root.
        /// </summary>
        /// <param name="alter">The function that alters the document root.</param>
        /// <returns></returns>
        public DocumentState AlterDocumentRoot(Func<CompilationUnitSyntax, CompilationUnitSyntax> alter)
            => AlterDocument(doc => doc.AlterRoot(alter));

        /// <summary>
        /// Gets a new instance of DocumentState with the specified changed project.
        /// </summary>
        /// <param name="alter">The function that alters the project.</param>
        /// <returns></returns>
        public Result<DocumentState> AlterProject(Func<Project, Result<Project>> alter)
            => alter(_doc.Project)
            .ToTypedResult(proj => new DocumentState(proj.GetDocument(_doc.Id)));

        /// <summary>
        /// Gets the document context.
        /// </summary>
        /// <returns></returns>
        private Result<DocumentContext> GetDocumentContext()
            => ProjectContext.Create(_doc.Project)
            .ToTypedResult(projCtx => projCtx.GetDocumentContext(_doc.Id));

        /// <summary>
        /// Gets the project context.
        /// </summary>
        /// <returns></returns>
        private Result<ProjectContext> GetProjectContext()
            => ProjectContext.Create(_doc.Project);
    }
}