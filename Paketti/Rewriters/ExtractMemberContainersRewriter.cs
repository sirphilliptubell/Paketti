using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Paketti.Contexts;
using Paketti.Extensions;
using Paketti.Utilities;

namespace Paketti.Rewriters
{
    internal class ExtractMemberContainersRewriter
    {
        private readonly IPackageContentSelector _contentSelector;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractMemberContainersRewriter"/> class.
        /// </summary>
        /// <param name="contentSelector">The content selector.</param>
        /// <exception cref="System.ArgumentException">contentSelector</exception>
        public ExtractMemberContainersRewriter(IPackageContentSelector contentSelector)
        {
            _contentSelector = contentSelector ?? throw new ArgumentException(nameof(contentSelector));
        }

        /// <summary>
        /// Gets the classes that were extracted.
        /// </summary>
        public IEnumerable<ExtractedClassInfo> ExtractedClasses { get; private set; }

        /// <summary>
        /// Gets the structs that were extracted.
        /// </summary>
        public IEnumerable<ExtractedStructInfo> ExtractedStructs { get; private set; }

        /// <summary>
        /// Extracts dependent members of a type and also removes them from the project.
        /// </summary>
        /// <param name="dependencyWalker">The dependency walker.</param>
        /// <returns></returns>
        public Result<Project> Rewrite(IDependencyWalker dependencyWalker)
        {
            var extracted = _contentSelector.GetMemberContainers(dependencyWalker);
            if (extracted.IsFailure)
                return Result.Fail<Project>(extracted.Error);

            this.ExtractedClasses = extracted.Value.classes;
            this.ExtractedStructs = extracted.Value.structs;

            return RemoveObjectsFromProject(dependencyWalker, extracted.Value.classes, extracted.Value.structs);
        }

        /// <summary>
        /// Removes the objects from project.
        /// </summary>
        /// <returns></returns>
        internal static Result<Project> RemoveObjectsFromProject(IDependencyWalker dependencyWalker, IEnumerable<ExtractedClassInfo> classes, IEnumerable<ExtractedStructInfo> structs)
        {
            //Unfortunately the Microsoft.CodeAnalysis.CSharp.CSharpSyntaxRewriter only rewrites a single document at a time.
            //The extracted members came from multiple documents (which have references to the SyntaxNodes they came from.)
            //The easiesy way to rewrite all the documents without other hassles is to precompute all the new roots for the documents
            //that we need to remove nodes from.

            var documents =
                dependencyWalker
                .ProjectContext
                .Documents
                .Select(x => x.Document)
                .ToList();

            //todo: validate the classes/structs actually belong to the documents in the dependencywalker.

            var modifications =
                documents
                //get each document and the declarations to remove in each
                .Select(x => new
                {
                    Document = x,

                    //combine the class and struct declarations for each document
                    Declarations =
                        classes
                        .Where(c => c.Document.Document == x)
                        .Select(c => c.Class.Declaration)
                        .OfType<SyntaxNode>()
                        .Union(
                            structs
                            .Where(s => s.Document.Document == x)
                            .Select(s => s.Struct.Declaration)
                        )
                })
                .Select(x => new
                {
                    DocumentId = x.Document.Id,
                    NewRoot = x.Document.GetRootSync().RemoveNodes(x.Declarations, SyntaxRemoveOptions.KeepNoTrivia)
                })
                .ToList();

            var newProject = dependencyWalker.ProjectContext.Project;
            foreach (var modification in modifications)
            {
                newProject =
                    newProject
                    .GetDocument(modification.DocumentId)
                    .AlterRoot(_ => modification.NewRoot)
                    .Project;
            }

            return newProject;
        }
    }
}