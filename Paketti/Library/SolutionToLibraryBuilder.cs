using System;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.FileProviders;
using Paketti.Contexts;
using Paketti.Extensions;
using Paketti.Logging;
using Paketti.Rewriters;
using Paketti.Utilities;

namespace Paketti.Library
{
    /// <summary>
    /// Builds a package library from a solution.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SolutionToLibraryBuilder<T> :
        ISolutionToLibraryBuilder<T>
        where T : Workspace
    {
        private readonly ICompiler _compiler;
        private readonly IFileInfo _solutionFile;
        private readonly Func<T> _workspaceFactory;
        private readonly Func<ProjectContext, IDependencyWalker> _walkerFactory;
        private readonly ILog _log;
        private readonly Action<T, string> _openSolution;

        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionToLibraryBuilder{T}"/> class.
        /// </summary>
        /// <param name="verifyingCompiler">The .net compiler used to verify builds.</param>
        /// <param name="solutionFile">The solution file.</param>
        /// <param name="workspaceFactory">The factory method that gets the workspace containing the solution.</param>
        /// <param name="log">The log.</param>
        /// <param name="openSolution">A method that opens the solution in the workspace, given the provided solution path (if required).</param>
        /// <exception cref="System.ArgumentException">
        /// verifyingCompiler
        /// or
        /// solutionFile
        /// or
        /// workspaceFactory
        /// or
        /// log
        /// </exception>
        public SolutionToLibraryBuilder(
            ICompiler verifyingCompiler,
            IFileInfo solutionFile,
            Func<T> workspaceFactory,
            Func<ProjectContext, IDependencyWalker> walkerFactory,
            ILog log, Action<T, string> openSolution = null)
        {
            _compiler = verifyingCompiler ?? throw new ArgumentException(nameof(verifyingCompiler));
            _solutionFile = solutionFile ?? throw new ArgumentException(nameof(solutionFile));
            _workspaceFactory = workspaceFactory ?? throw new ArgumentException(nameof(workspaceFactory));
            _walkerFactory = walkerFactory ?? throw new ArgumentException(nameof(walkerFactory));
            _log = log ?? throw new ArgumentException(nameof(log));

            _openSolution = openSolution;
        }

        /// <summary>
        /// Builds the package library.
        /// </summary>
        /// <returns></returns>
        public Result<ILibrary> Build()
            => GetWorkspaceFromSlnFile()
            .OnSuccess(CreateAdHocClone)
            .OnSuccess(RewriteAllProjectsInSolution)
            .OnSuccess(SolutionContext.Create)
            .OnSuccess(sol => sol.ProjectContext)
            .OnSuccess(CreateWalker)
            .OnSuccess(BuildLibrary);

        /// <summary>
        /// Creates an AdHocWorkspace clone of the original workspace.
        /// </summary>
        /// <param name="ws">The ws.</param>
        /// <returns></returns>
        private Result<AdhocWorkspace> CreateAdHocClone(Workspace ws)
            => ws.CreateClone();

        /// <summary>
        /// Creates the dependency walker for the project context.
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
        /// Bruilds the library.
        /// </summary>
        /// <param name="solutionContext">The solution context.</param>
        /// <returns></returns>
        private Result<ILibrary> BuildLibrary(IDependencyWalker walker)
            => new ProjectToLibraryBuilder()
            .Build(walker, new Library(), _log);

        /// <summary>
        /// Gets the workspace.
        /// </summary>
        /// <returns></returns>
        private Result<Workspace> GetWorkspaceFromSlnFile()
        {
            if (!_solutionFile.Name.ToLowerInvariant().EndsWith(".sln"))
                return Result.Fail<Workspace>("The specified solution file does not end with .sln");

            if (!_solutionFile.Exists)
                return Result.Fail<Workspace>($"Solution file {_solutionFile.PhysicalPath} doesn't exist.");

            var ws = _workspaceFactory();
            _openSolution?.Invoke(ws, _solutionFile.PhysicalPath);
            return ws;
        }

        /// <summary>
        /// Rewrites all the projects in the solution.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <returns></returns>
        private Result<Workspace> RewriteAllProjectsInSolution(Workspace workspace)
            => new SolutionRewriter(_compiler, _log)
            .Rewrite(workspace);
    }
}