using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.FileProviders;
using Paketti.Library;
using Paketti.Logging;
using Paketti.Rewriters;
using Paketti.Utilities;
using System;
using System.Diagnostics;
using System.Linq;

namespace Paketti.Cook
{
    internal class Program
    {
        const string solutionDrive = @"D:\";
        const string solutionPath = @"Repositories\DotNetFunctional\DotNetFunctional.sln";

        private static void Main(string[] args)
        {
            /* TODO
             * Add InterfaceContext
             * Add Delegates to Class/Struct/Interface Contexts
             * Ensure Class/Struct/Interface Contexts's GetTypeDependencies() also check the types that they inherit from, eg: other classes/structs/etc..
             *
             * If the paketti package contains extension methods that do not use any interweaves. Then most likely the extension method is a helper for
             * some interweave in the paketti project. In which case when a user imports that interweave, we should create an internal extension method class
             * which contains these helper methods. The interweave may need to be re-written to explicitly use that extension method. We don't want to
             * pollute the consumer's namespace with extension methods with ones that might collide with their own.
             *
             * Find and remove all debugger.break()s
             * */
             
            Action<Project> display = DisplayProject;
            
            var compiler = new Compiler();
            var log = new ConsoleLog();
            var solutionFile = new PhysicalFileProvider(solutionDrive).GetFileInfo(solutionPath);
            var solutionRewriter = new SolutionRewriter(log);
            var builder = new SolutionToLibraryBuilder<MSBuildWorkspace>(compiler, solutionFile, MSBuildWorkspace.Create, solutionRewriter,
                pc => new DependencyWalker(pc, log),
                new PackageContentSelector(),
                display,
                log,
                (ws, path) => ws.OpenSolutionAsync(path).Result);
            var result = builder.Build();
            
            log.LogStep("Done");
            Console.ReadKey();
        }

        private static void VerifySolutionOpens()
        {
            var ws = MSBuildWorkspace.Create();

            ws.WorkspaceFailed += MSBuildWorkspace_WorkspaceFailed;

            //In the event that assemblies are missing or nuget packages are incompatible, this will generally supply some error.
            var solution = ws.OpenSolutionAsync(solutionDrive + solutionPath).Result;
        }

        private static void MSBuildWorkspace_WorkspaceFailed(object sender, WorkspaceDiagnosticEventArgs e)
        {
            Console.WriteLine(e.Diagnostic.Message);
            Debugger.Break();
        }

        private static void DisplayProject(Project p)
        {
            var remainingDocuments = p
                .Documents
                .Select(x => new { Name = x.Name, ZContents = x.GetTextAsync().Result.ToString() })
                .ToList();

            Debugger.Break();
        }
    }
}