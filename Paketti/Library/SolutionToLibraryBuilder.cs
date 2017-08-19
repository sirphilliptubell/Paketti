using System;
using System.Collections.Generic;
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
        private readonly ISolutionRewriter _solutionRewriter;
        private readonly Func<ProjectContext, IDependencyWalker> _walkerFactory;
        private readonly ILog _log;
        private readonly Func<T, string, Solution> _openSolution;
        private readonly IPackageContentSelector _contentSelector;
        private readonly Maybe<Action<Project>> _analyzeResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionToLibraryBuilder{T}" /> class.
        /// </summary>
        /// <param name="verifyingCompiler">The .net compiler used to verify builds.</param>
        /// <param name="solutionFile">The solution file.</param>
        /// <param name="workspaceFactory">The factory method that gets the workspace containing the solution.</param>
        /// <param name="solutionRewriter">The solution rewriter.</param>
        /// <param name="walkerFactory">The walker factory.</param>
        /// <param name="contentSelector">The content selector.</param>
        /// <param name="assemblyAnalyzer">The assembly analyzer.</param>
        /// <param name="log">The log.</param>
        /// <param name="openSolution">A method that opens the solution in the workspace, given the provided solution path (if required).</param>
        /// <exception cref="System.ArgumentException">verifyingCompiler
        /// or
        /// solutionFile
        /// or
        /// workspaceFactory
        /// or
        /// log</exception>
        /// <exception cref="System.ArgumentNullException">
        /// assemblyAnalyzer
        /// or
        /// contentSelector
        /// </exception>
        public SolutionToLibraryBuilder(
            ICompiler verifyingCompiler,
            IFileInfo solutionFile,
            Func<T> workspaceFactory,
            ISolutionRewriter solutionRewriter,
            Func<ProjectContext, IDependencyWalker> walkerFactory,
            IPackageContentSelector contentSelector,
            Maybe<Action<Project>> analyzeResult,
            ILog log,
            Func<T, string, Solution> openSolution = null)
        {
            _compiler = verifyingCompiler ?? throw new ArgumentException(nameof(verifyingCompiler));
            _solutionFile = solutionFile ?? throw new ArgumentException(nameof(solutionFile));
            _workspaceFactory = workspaceFactory ?? throw new ArgumentException(nameof(workspaceFactory));
            _solutionRewriter = solutionRewriter ?? throw new ArgumentException(nameof(solutionRewriter));
            _walkerFactory = walkerFactory ?? throw new ArgumentException(nameof(walkerFactory));
            _log = log ?? throw new ArgumentException(nameof(log));
            _contentSelector = contentSelector ?? throw new ArgumentNullException(nameof(contentSelector));

            _analyzeResult = analyzeResult;
            _openSolution = openSolution;
        }

        /// <summary>
        /// Builds the package library.
        /// </summary>
        /// <returns></returns>
        public Result<ILibrary> Build()
            //get and clone the workspace
            => GetWorkspaceFromSlnFile()
            .OnSuccess(CreateAdHocClone)

            //Verify it compiles
            .OnSuccess(SolutionContext.Create)
            .OnSuccess(VerifySolutionCompiles)

            //finish
            .OnSuccess(sol => sol.ProjectContext)
            .OnSuccess(BuildLibrary);

        /// <summary>
        /// Creates an AdHocWorkspace clone of the original workspace.
        /// </summary>
        /// <param name="ws">The ws.</param>
        /// <returns></returns>
        private Result<AdhocWorkspace> CreateAdHocClone(Solution solution)
            => solution.CloneIntoWorkspace();

        /// <summary>
        /// Bruilds the library.
        /// </summary>
        /// <param name="solutionContext">The solution context.</param>
        /// <returns></returns>
        private Result<ILibrary> BuildLibrary(ProjectContext projectContext)
            => new ProjectToLibraryBuilder(new Library(), _contentSelector, _solutionRewriter, _walkerFactory, _log, _analyzeResult)
            .Build(projectContext);

        /// <summary>
        /// Gets the workspace.
        /// </summary>
        /// <returns></returns>
        private Result<Solution> GetWorkspaceFromSlnFile()
        {
            if (!_solutionFile.Name.ToLowerInvariant().EndsWith(".sln"))
                return Result.Fail<Solution>("The specified solution file does not end with .sln");

            if (!_solutionFile.Exists)
                return Result.Fail<Solution>($"Solution file {_solutionFile.PhysicalPath} doesn't exist.");

            var ws = _workspaceFactory();
            var solution = _openSolution?.Invoke(ws, _solutionFile.PhysicalPath);
            return solution;
        }

        /// <summary>
        /// Verifies the solution compiles.
        /// </summary>
        /// <param name="solutionContext">The solution context.</param>
        /// <returns></returns>
        private Result<SolutionContext> VerifySolutionCompiles(SolutionContext solutionContext)
            => _compiler
            .Compile(solutionContext.ProjectContext, _log)
            .ToTypedResult(solutionContext);
    }
}