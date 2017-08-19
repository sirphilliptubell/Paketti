using System;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Paketti.Contexts;
using Paketti.Logging;
using Paketti.Utilities;

namespace Paketti.Cook
{
    /// <summary>
    /// .Net Compiler.
    /// </summary>
    public class Compiler :
        ICompiler
    {
        /// <summary>
        /// Compiles the specified project.
        /// </summary>
        /// <param name="projectContext"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public Result Compile(ProjectContext projectContext, ILog log)
        {
            using (log.LogStep("Compiling"))
            {
                var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
                var systemLinq = MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location);
                var runtimeCompilerServices = MetadataReference.CreateFromFile(typeof(System.Runtime.CompilerServices.TupleElementNamesAttribute).Assembly.Location);
                var runtime = MetadataReference.CreateFromFile(@"c:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.Runtime.dll");
                var references = projectContext.Project.MetadataReferences;

                var compilation = CSharpCompilation.Create("TestCompilation", projectContext.DocumentSyntaxTrees, references, options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

                using (var outputStream = new MemoryStream())
                {
                    var emitResult = compilation.Emit(outputStream);

                    if (emitResult.Success)
                        return Result.Ok();
                    else
                    {
                        var sb = new StringBuilder();
                        foreach (var diagnostic in emitResult.Diagnostics)
                        {
                            if (sb.Length == 0)
                                sb.Append(diagnostic);
                            else
                                sb.Append(Environment.NewLine).Append(diagnostic);
                        }
                        return Result.Fail(sb.ToString());
                    }
                }
            }
        }
    }
}