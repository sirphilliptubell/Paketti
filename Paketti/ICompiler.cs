using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Paketti.Contexts;

namespace Paketti
{
    public interface ICompiler
    {
        Result Compile(ProjectContext projectContext);
    }
}