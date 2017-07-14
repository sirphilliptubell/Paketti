﻿using System;
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
        private readonly IPackageContentSelector _contentSelector;
        private readonly ISolutionRewriter _solutionRewriter;
        private readonly Func<ProjectContext, IDependencyWalker> _walkerFactory;
        private readonly ILog _log;
        private readonly Maybe<Action<Project>> _analyzeResult;

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
        public ProjectToLibraryBuilder(ILibrary library, IPackageContentSelector contentSelector, ISolutionRewriter solutionRewriter, Func<ProjectContext, IDependencyWalker> walkerFactory, ILog log, Maybe<Action<Project>> analyzeResult)
        {
            _library = library ?? throw new ArgumentException(nameof(library));
            _contentSelector = contentSelector ?? throw new ArgumentException(nameof(contentSelector));
            _solutionRewriter = solutionRewriter ?? throw new ArgumentException(nameof(solutionRewriter));
            _walkerFactory = walkerFactory ?? throw new ArgumentException(nameof(walkerFactory));
            _log = log ?? throw new ArgumentException(nameof(log));
            _analyzeResult = analyzeResult;
        }

        /// <summary>
        /// Builds the package library.
        /// </summary>
        /// <returns></returns>
        public Result<ILibrary> Build(ProjectContext projectContext)
            =>
            //cleanup
            MakeTypesPartial(projectContext)
            .OnSuccess(RemoveRegionDirectives)
            .OnSuccess(RemoveUnusedUsingDirectives)

            //pull out extension methods which depend on interweaves
            .OnSuccess(CreateWalker)
            .OnSuccess(ExtractInterwovenExtensionMethods)

            //pull out members which depend on interweaves
            .OnSuccess(CreateWalker)
            .OnSuccess(ExtractInterwovenTypeMembers)

            //pull out remaining types
            .OnSuccess(CreateWalker)
            .OnSuccess(ExtractMemberContainers)

            //cleanup
            .OnSuccess(p => ProjectContext.Create(p))
            .OnSuccess(RemoveUnusedUsingDirectives)

            //allow verification if needed
            .OnSuccess(pc => pc.Project)
            .OnSuccess(AfterRewrites)

            .OnSuccess(() => Result.Ok(_library));

        /// <summary>
        /// Calls the afterRewrites action provided by the constructor.
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        private Result<Project> AfterRewrites(Project project)
        {
            if (_analyzeResult.HasValue)
                _analyzeResult.Value(project);

            return project;
        }

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
        /// Removes the unused/duplicate Using Directives.
        /// </summary>
        /// <param name="projectContext">The project context.</param>
        /// <returns></returns>
        private Result<ProjectContext> RemoveUnusedUsingDirectives(ProjectContext projectContext)
            => _solutionRewriter.Rewrite(projectContext, new RemoveUnusedUsingsRewriter());

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
        private Result<Project> ExtractInterwovenExtensionMethods(IDependencyWalker dependencyWalker)
        {
            var rewriter = new ExtractInterwovenExtensionMethodsRewriter(_contentSelector);

            var result = rewriter.Rewrite(dependencyWalker);
            if (result.IsFailure)
                return result;

            var membersAndDependencies =
                rewriter.
                ExtractedMembers
                .Select(x => new
                {
                    ExtensionMethod = x,
                    Descriptions = new InterweaveDescriptions(dependencyWalker.GetTypeDependencies(x).OnlyInterweaves())
                })
                .GroupBy(x => x.Descriptions.Key)
                .ToList();

            foreach (var memberAndDependencies in membersAndDependencies)
            {
                foreach (var item in memberAndDependencies)
                {
                    _library.AddOrMerge(new InterwovenExtensionMethodsPackage(item.ExtensionMethod.Declaration.ToFormattedCode(), item.Descriptions));
                }
            }

            return result;
        }

        /// <summary>
        /// Removes optional members for types while adding them to the library.
        /// </summary>
        /// <param name="walker">The walker.</param>
        /// <returns></returns>
        private Result<Project> ExtractInterwovenTypeMembers(IDependencyWalker dependencyWalker)
        {
            //this rewriter will collect the information it removed during the rewrite.
            var rewriter = new ExtractInterwovenTypeMembersRewriter(_contentSelector);

            var result = rewriter.Rewrite(dependencyWalker);
            if (result.IsFailure)
                return result;

            var membersAndDependencies =
                rewriter
                .ExtractedMembers
                .Select(x => new
                {
                    Member = x,
                    Descriptions = new InterweaveDescriptions(dependencyWalker.GetTypeDependencies(x).OnlyInterweaves())
                })
                .GroupBy(x => x.Descriptions.Key)
                .ToList();

            foreach (var memberAndDependency in membersAndDependencies)
            {
                foreach (var item in memberAndDependency)
                {
                    _library.AddOrMerge(new InterwovenTypeMembersPackage(item.Member.ContainingTypeContext.Value.Key, item.Member.Declaration.ToFormattedCode(), item.Descriptions));
                }
            }

            return result;
        }

        /// <summary>
        /// Removes member containers while adding them to the library.
        /// </summary>
        /// <param name="walker">The walker.</param>
        /// <returns></returns>
        private Result<Project> ExtractMemberContainers(IDependencyWalker dependencyWalker)
        {
            var rewriter = new ExtractMemberContainersRewriter(_contentSelector);

            var result = rewriter.Rewrite(dependencyWalker);
            if (result.IsFailure)
                return result;

            Func<Document, IEnumerable<string>> getUsings = doc => doc.GetUsings().Select(x => x.Name.ToString());

            foreach (var cls in rewriter.ExtractedClasses)
            {
                var usings = getUsings(cls.Document.Document);
                var name = cls.Class.Name;
                var genericArgCount = cls.Class.TypeArguments.Count();
                var members = cls.Class.Declaration.DescendantNodesOfFirstLevel();
                var declaration = cls.Class.Declaration.RemoveNodes(members, SyntaxRemoveOptions.KeepNoTrivia).ToFullString();
                var content = string.Join(Environment.NewLine + Environment.NewLine, members.Select(x => x.ToFullString()));

                _library.AddOrMerge(new MemberContainerPackage(MemberContainerKind.Class, usings, name, (byte)genericArgCount, declaration, content));
            }

            foreach (var str in rewriter.ExtractedStructs)
            {
                var usings = getUsings(str.Document.Document);
                var name = str.Struct.Name;
                var genericArgCount = str.Struct.TypeArguments.Count();
                var members = str.Struct.Declaration.DescendantNodesOfFirstLevel();
                var declaration = str.Struct.Declaration.RemoveNodes(members, SyntaxRemoveOptions.KeepNoTrivia).ToFullString();
                var content = string.Join(Environment.NewLine + Environment.NewLine, members.Select(x => x.ToFullString()));

                _library.AddOrMerge(new MemberContainerPackage(MemberContainerKind.Struct, usings, name, (byte)genericArgCount, declaration, content));
            }

            return result;
        }
    }
}