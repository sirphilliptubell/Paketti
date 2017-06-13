using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Paketti.Contexts
{
    public class ProjectContext
    {
        public Project Project { get; }

        public IEnumerable<DocumentContext> Documents { get; }

        private ProjectContext(Project project)
        {
            this.Project = project ?? throw new ArgumentNullException(nameof(project));

            var compilation = this.NewCompilation;
            this.Documents = Project
                .Documents
                .Select(x => new DocumentContext(this, Project, x, compilation))
                .ToList();
        }

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

        public IEnumerable<SyntaxTree> DocumentSyntaxTrees
            => Project.Documents.Select(x => x.GetSyntaxTreeAsync().Result);

        public CSharpCompilation NewCompilation
            => CSharpCompilation.Create(Project.AssemblyName, DocumentSyntaxTrees);

        public IEnumerable<ISymbol> GetProjectSymbols()
            => Documents.SelectMany(x => x.Classes.Select(y => y.Symbol))
            .Union(Documents.SelectMany(x => x.Structs.Select(y => y.Symbol)))
            .Union(Documents.SelectMany(x => x.Delegates.Select(y => y.Symbol)));

        public IEnumerable<IClassOrStruct> GetClassesAndStructs()
            => Documents.SelectMany(x => x.GetClassesAndStructs());

        public IEnumerable<MethodContext> GetExtensionMethods()
            => Documents
            .SelectMany(x => x.GetExtensionMethods());
    }
}