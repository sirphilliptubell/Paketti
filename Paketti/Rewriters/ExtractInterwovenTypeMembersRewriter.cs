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
    /// Finds instance/static type members which depend on interwoven types, and removes them from the documents.
    /// Note: Does not include extension methods.
    /// </summary>
    internal class ExtractInterwovenTypeMembersRewriter
    {
        private readonly IPackageContentSelector _contentSelector;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractInterwovenTypeMembersRewriter"/> class.
        /// </summary>
        /// <param name="contentSelector">The content selector.</param>
        /// <exception cref="System.ArgumentException">contentSelector</exception>
        public ExtractInterwovenTypeMembersRewriter(IPackageContentSelector contentSelector)
        {
            _contentSelector = contentSelector ?? throw new ArgumentException(nameof(contentSelector));
        }

        /// <summary>
        /// Gets the Type Members that were extracted.
        /// </summary>
        public IEnumerable<ITypeMemberContext> ExtractedMembers { get; private set; }

        /// <summary>
        /// Extracts dependent members of a type and also removes them from the project.
        /// </summary>
        /// <param name="dependencyWalker">The dependency walker.</param>
        /// <returns></returns>
        public Result<Project> Rewrite(IDependencyWalker dependencyWalker)
        {
            var extracted = _contentSelector.GetInterwovenTypeMembersExceptExtensions(dependencyWalker);
            if (extracted.IsFailure)
                return Result.Fail<Project>(extracted.Error);
            else
                ExtractedMembers = extracted.Value;

            return RemoveObjectsFromProject(dependencyWalker, ExtractedMembers);
        }

        /// <summary>
        /// Removes the objects from project.
        /// </summary>
        /// <param name="dependencyWalker">The dependency walker.</param>
        /// <param name="extractedMembers">The extracted members.</param>
        /// <returns></returns>
        internal static Result<Project> RemoveObjectsFromProject(IDependencyWalker dependencyWalker, IEnumerable<ITypeMemberContext> extractedMembers)
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

            var ensureResult = extractedMembers.EnsureExistInDocuments(documents);
            if (ensureResult.IsFailure)
                return ensureResult.ToTypedResult<Project>();

            var modifications =
                extractedMembers
                .Select(m => new
                {
                    Member = m,
                    Doc = documents.Where(d => d.ContainsNode(m.Declaration)).Single()
                })
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