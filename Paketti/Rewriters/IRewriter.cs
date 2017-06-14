using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Paketti.Contexts;

namespace Paketti.Rewriters
{
    public interface IRewriter
    {
        Document Rewrite(DocumentContext document);

        /// <summary>
        /// Gets a value indicating whether the project should be recompiled to validate the changes
        /// didn't break the project.
        /// </summary>
        /// <value>
        ///   <c>true</c> if you should recompile to validate the project; otherwise, <c>false</c>.
        /// </value>
        bool ShouldRecompileToValidate { get; }
    }
}