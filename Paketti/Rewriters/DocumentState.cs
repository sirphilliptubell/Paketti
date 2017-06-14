using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Paketti.Contexts;
using Paketti.Extensions;

namespace Paketti.Rewriters
{
    internal class DocumentState
    {
        private readonly Document _doc;

        public DocumentState(Document document)
        {
            _doc = document ?? throw new ArgumentNullException(nameof(document));
        }

        public DocumentState(Project project, DocumentId documentId)
            : this(project?.GetDocument(documentId))
        { }

        public DocumentState(DocumentContext documentContext)
            : this(documentContext?.Document)
        { }

        public Document Document
            => _doc;

        public DocumentState AlterDocument(Func<Document, Document> alter)
            => new DocumentState(alter(_doc));

        public Result<DocumentState> AlterDocument(Func<Document, Result<Document>> alter)
            => alter(_doc)
            .ToTypedResult(doc => new DocumentState(doc));

        public DocumentState AlterDocumentRoot(Func<CompilationUnitSyntax, CompilationUnitSyntax> alter)
                    => AlterDocument(doc => doc.AlterRoot(alter));

        public Result<DocumentState> AlterProject(Func<Project, Result<Project>> alter)
            => alter(_doc.Project)
            .ToTypedResult(proj => new DocumentState(proj.GetDocument(_doc.Id)));

        private Result<DocumentContext> GetDocumentContext()
            => ProjectContext.Create(_doc.Project)
            .ToTypedResult(projCtx => projCtx.GetDocumentContext(_doc.Id));

        private Result<ProjectContext> GetProjectContext()
                    => ProjectContext.Create(_doc.Project);
    }
}