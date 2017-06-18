using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Paketti.Contexts;
using Paketti.Extensions;
using Paketti.Utilities;

namespace Paketti.Rewriters
{
    /// <summary>
    /// Finds type members which have local dependencies, and removes them from the documents.
    /// </summary>
    internal class ExtractTypeDependenciesRewriter
    {
        /// <summary>
        /// Gets the Type Members that were extracted.
        /// </summary>
        public IEnumerable<ITypeMemberContext> ExtractedMembers { get; private set; }

        /// <summary>
        /// Rewrites the ProjectContext within the specified DependencyWalker.
        /// </summary>
        /// <param name="dependencyWalker">The dependency walker.</param>
        /// <returns></returns>
        public Result<Project> Rewrite(IDependencyWalker dependencyWalker)
        {
            //The goal is to find all the methods/properties/etc.. which are dependent on local types.
            //Then remove those from all the documents
            //While storing those for the result
            //And return the new ProjectContext

            //The CSharpSyntaxRewriter unfortunately only modifies one document at a time
            //We can't just remove the dependencies from one document and then create a new ProjectContext, because that likely won't build
            //So get all the type members at once and replace all the documents using only the single ProjectContext.

            ExtractedMembers =
                dependencyWalker
                .ProjectContext
                .GetTypeMembersExcludingExtensions()
                //must have local dependencies
                .Where(x => dependencyWalker.GetTypeDependencies(x).OnlyLocal().Any())
                .ToList();

            var missingContainingDeclaration =
                ExtractedMembers
                .Where(x => x.ContainingTypeContext.HasNoValue);
            if (missingContainingDeclaration.Any())
                return Result.Fail<Project>("Some type members are missing a containing type: " + missingContainingDeclaration.Select(x => x.Declaration.ToString()));

            return RemoveObjectsFromProject(dependencyWalker, ExtractedMembers);
        }

        internal static Result<Project> RemoveObjectsFromProject(IDependencyWalker dependencyWalker, IEnumerable<ITypeMemberContext> extractedMembers)
        {
            var docs =
                dependencyWalker
                .ProjectContext
                .Documents
                .Select(x => x.Document)
                .ToList();

            var ensureResult = extractedMembers.EnsureExistInDocuments(docs);
            if (ensureResult.IsFailure)
                return ensureResult.ToTypedResult<Project>();

            var member2Doc =
                extractedMembers
                .Select(x => new
                {
                    Member = x,
                    Doc = docs.Where(d => d.ContainsNode(x.Declaration)).Single()
                })
                .ToList();

            var modifications =
                member2Doc
                .GroupBy(x => x.Doc)
                .Select(x => new
                {
                    Document = x.Key,
                    Declarations = x.Select(mc => mc.Member.Declaration).ToList()
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