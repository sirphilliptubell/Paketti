using System;
using Paketti.Contexts;
using Paketti.Logging;

namespace Paketti.Utilities
{
    public interface ICompiler
    {
        Result Compile(ProjectContext projectContext, ILog log);
    }
}