using System;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.FileProviders;
using Paketti.Contexts;
using Paketti.Extensions;
using Paketti.Logging;
using Paketti.Packaging;
using Paketti.Rewriters;

namespace Paketti.Utilities
{
    public class PackageBuilder<T>
        where T : Workspace
    {
        private readonly ICompiler _compiler;
        private readonly ILog _log;

        public PackageBuilder(ICompiler compiler, ILog log)
        {
            _compiler = compiler ?? throw new ArgumentException(nameof(compiler));
            _log = log ?? throw new ArgumentException(nameof(log));
        }

        public Result<IPackageStore> CreatePackage(IFileInfo solutionFile, Func<T> workspaceFactory, Action<T, string> openSolution = null)
            => GetWorkspace(solutionFile, workspaceFactory, openSolution)
            .OnSuccess(CreateAdHocClone)
            .OnSuccess(Rewrite)
            .OnSuccess(SolutionContext.Create)
            .OnSuccess(CreatePackage);

        private Result<AdhocWorkspace> CreateAdHocClone(Workspace ws)
            => ws.CreateClone();

        private Result<IPackageStore> CreatePackage(SolutionContext solutionContext)
            => new Packager(solutionContext.ProjectContext, new PackageStore(), pc => new DependencyWalker(pc, _log), _log)
            .Pack();

        private Result<Workspace> GetWorkspace(IFileInfo solutionFile, Func<T> workspaceFactory, Action<T, string> openSolution = null)
        {
            if (!solutionFile.Name.ToLowerInvariant().EndsWith(".sln"))
                return Result.Fail<Workspace>("The specified solution file does not end with .sln");

            if (!solutionFile.Exists)
                return Result.Fail<Workspace>($"Solution file {solutionFile.PhysicalPath} doesn't exist.");

            var ws = workspaceFactory();
            openSolution?.Invoke(ws, solutionFile.PhysicalPath);
            return ws;
        }

        private Result<Workspace> Rewrite(Workspace workspace)
            => new SolutionRewriter(_compiler, _log)
            .Rewrite(workspace);
    }
}