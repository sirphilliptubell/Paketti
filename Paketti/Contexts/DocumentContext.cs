using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Paketti.Contexts
{
    public class DocumentContext
    {
        public Project Project { get; }

        public ProjectContext ProjectContext { get; }

        public Document Document { get; }

        public CSharpCompilation Compilation { get; }

        public IEnumerable<ClassContext> Classes { get; }

        public IEnumerable<StructContext> Structs { get; }

        public IEnumerable<DelegateContext> Delegates { get; }

        public DocumentContext(ProjectContext projectContext, Project project, Document document, CSharpCompilation compilation)
        {
            ProjectContext = projectContext ?? throw new ArgumentException(nameof(projectContext));
            Project = project ?? throw new ArgumentException(nameof(project));
            Document = document ?? throw new ArgumentException(nameof(document));
            Compilation = compilation ?? throw new ArgumentException(nameof(compilation));

            Classes = SyntaxRoot
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Select(x => new ClassContext(x, compilation, SemanticModel))
                .ToList();

            Structs = SyntaxRoot
                .DescendantNodes()
                .OfType<StructDeclarationSyntax>()
                .Select(x => new StructContext(x, compilation, SemanticModel))
                .ToList();

            Delegates = SyntaxRoot
                .DescendantNodes()
                .OfType<DelegateDeclarationSyntax>()
                .Select(x => new DelegateContext(x, compilation, SemanticModel))
                .ToList();
        }

        public SyntaxNode SyntaxRoot
            => Document.GetSyntaxRootAsync().Result;

        public SemanticModel SemanticModel
            => Document.GetSemanticModelAsync().Result;

        public SyntaxTree SyntaxTree
            => Document.GetSyntaxTreeAsync().Result;

        public IEnumerable<IClassOrStruct> GetClassesAndStructs()
            => ((IEnumerable<IClassOrStruct>)Classes)
            .Union(Structs);

        public IEnumerable<MethodContext> GetExtensionMethods()
            => Classes
            .Where(x => x.IsStatic)
            .SelectMany(x => x.Methods)
            .Where(x => x.IsExtensionMethod);

        public override string ToString()
        {
            return $"classes:{Classes.Count()}, structs:{Structs.Count()} {Document.FilePath}";
        }
    }
}