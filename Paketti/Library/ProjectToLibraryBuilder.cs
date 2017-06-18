using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Paketti.Contexts;
using Paketti.Extensions;
using Paketti.Logging;
using Paketti.Rewriters;
using Paketti.Utilities;

namespace Paketti.Library
{
    /// <summary>
    /// Builds a library of packages from a ProjectContext.
    /// </summary>
    /// <seealso cref="Paketti.Packaging.IPackager" />
    public class ProjectToLibraryBuilder :
        IProjectToLibraryBuilder
    {
        private readonly ILibrary _library;
        private readonly ISolutionRewriter _solutionRewriter;
        private readonly Func<ProjectContext, IDependencyWalker> _walkerFactory;
        private readonly ILog _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectToLibraryBuilder"/> class.
        /// </summary>
        /// <param name="library">The library.</param>
        /// <param name="solutionRewriter">The solution rewriter.</param>
        /// <param name="walkerFactory">The walker factory.</param>
        /// <param name="log">The log.</param>
        /// <exception cref="System.ArgumentException">
        /// library
        /// or
        /// solutionRewriter
        /// or
        /// walkerFactory
        /// or
        /// log
        /// </exception>
        public ProjectToLibraryBuilder(ILibrary library, ISolutionRewriter solutionRewriter, Func<ProjectContext, IDependencyWalker> walkerFactory, ILog log)
        {
            _library = library ?? throw new ArgumentException(nameof(library));
            _solutionRewriter = solutionRewriter ?? throw new ArgumentException(nameof(solutionRewriter));
            _walkerFactory = walkerFactory ?? throw new ArgumentException(nameof(walkerFactory));
            _log = log ?? throw new ArgumentException(nameof(log));
        }

        /// <summary>
        /// Builds the package library.
        /// </summary>
        /// <returns></returns>
        public Result<ILibrary> Build(ProjectContext projectContext)
            => MakeTypesPartial(projectContext)
            .OnSuccess(RemoveRegionDirectives)
            .OnSuccess(CreateWalker)
            .OnSuccess(ExtractExtensionMethods)
            .OnSuccess(CreateWalker)
            .OnSuccess(ExtractTypeDependentMembers)
            .OnSuccess(() => Result.Ok(_library));

        /// <summary>
        /// Ensures all the types in the project have the partial modifier.
        /// </summary>
        /// <param name="projectContext">The project context.</param>
        /// <returns></returns>
        private Result<ProjectContext> MakeTypesPartial(ProjectContext projectContext)
            => _solutionRewriter.Rewrite(projectContext, new EnsureTopLevelClassesAndStructsArePartialRewriter());

        /// <summary>
        /// Removes all region and endregion directives from the project.
        /// </summary>
        /// <param name="projectContext">The project context.</param>
        /// <returns></returns>
        private Result<ProjectContext> RemoveRegionDirectives(ProjectContext projectContext)
            => _solutionRewriter.Rewrite(projectContext, new RemoveRegionRewriter());

        /// <summary>
        /// Creates a <see cref="IDependencyWalker"/> from a <see cref="ProjectContext"/>
        /// </summary>
        /// <param name="solutionContext">The solution context.</param>
        /// <returns></returns>
        private Result<IDependencyWalker> CreateWalker(ProjectContext projectContext)
        {
            var walker = _walkerFactory(projectContext);
            if (walker == null)
                return Result.Fail<IDependencyWalker>("The DependencyWalkerFactory function given in the constructor returned null.");
            else
                return Result.Ok(walker);
        }

        /// <summary>
        /// Creates a <see cref="IDependencyWalker"/> from a <see cref="Project"/>
        /// </summary>
        /// <param name="project">The project.</param>
        /// <returns></returns>
        private Result<IDependencyWalker> CreateWalker(Project project)
            => ProjectContext.Create(project)
            .OnSuccess(CreateWalker);

        /// <summary>
        /// Removes optional members for types while adding them to the library.
        /// </summary>
        /// <param name="walker">The walker.</param>
        /// <returns></returns>
        private Result<Project> ExtractExtensionMethods(IDependencyWalker dependencyWalker)
        {
            var rewriter = new ExtractExtensionMethodsRewriter();

            var newProject = rewriter.Rewrite(dependencyWalker);

            var membersAndDependencies =
                rewriter.
                ExtractedMembers
                .Select(x => new
                {
                    ExtensionMethod = x,
                    Dependencies = dependencyWalker.GetTypeDependencies(x).OnlyLocal()
                })
                .GroupBy(x => x.Dependencies.GetCollectiveOrderedKey())
                .ToList();

            foreach (var memberAndDependencies in membersAndDependencies)
            {
                foreach (var item in memberAndDependencies)
                {
                    var dependencies = item.Dependencies.Select(x => x.Key);
                    _library.AddOrMerge(new ExtensionMethodsPackage(item.ExtensionMethod.Declaration.ToFormattedCode(), dependencies));
                }
            }

            return newProject;
        }

        /// <summary>
        /// Removes optional members for types while adding them to the library.
        /// </summary>
        /// <param name="walker">The walker.</param>
        /// <returns></returns>
        private Result<Project> ExtractTypeDependentMembers(IDependencyWalker walker)
        {
            //this rewriter will collect the information it removed during the rewrite.
            var rewriter = new ExtractTypeDependenciesRewriter();

            var result = rewriter.Rewrite(walker);

            var membersAndDependencies =
                rewriter
                .ExtractedMembers
                .Select(x => new
                {
                    Member = x,
                    Dependencies = walker.GetTypeDependencies(x).OnlyLocal()
                })
                .GroupBy(keySelector: x => x.Dependencies.OnlyLocal().GetCollectiveOrderedKey())
                .ToList();

            foreach (var memberAndDependency in membersAndDependencies)
            {
                foreach (var item in memberAndDependency)
                {
                    var dependencies = item.Dependencies.Select(x => x.Key);
                    _library.AddOrMerge(new OptionalTypeMembersPackage(item.Member.ContainingTypeContext.Value.Key, item.Member.Declaration.ToFormattedCode(), dependencies));
                }
            }

            return result;
        }
    }
}