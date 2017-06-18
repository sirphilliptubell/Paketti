using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Paketti.Contexts;

namespace Paketti.Rewriters
{
    /// <summary>
    /// Handles rewriting solutions.
    /// </summary>
    public interface ISolutionRewriter
    {
        /// <summary>
        /// Rewrites the specified workspace.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <param name="rewriters">The rewriters.</param>
        /// <returns></returns>
        Result<Workspace> Rewrite(Workspace workspace, IEnumerable<IRewriter> rewriters);

        /// <summary>
        /// Rewrites the specified solution.
        /// </summary>
        /// <param name="originalSolution">The solution.</param>
        /// <param name="rewriter">The rewriter.</param>
        /// <returns></returns>
        Result<Solution> Rewrite(Solution originalSolution, IRewriter rewriter);

        /// <summary>
        /// Rewrites the specified original project context.
        /// </summary>
        /// <param name="originalProjectContext">The original project context.</param>
        /// <param name="rewriter">The rewriter.</param>
        /// <returns></returns>
        Result<ProjectContext> Rewrite(ProjectContext originalProjectContext, IRewriter rewriter);

        /// <summary>
        /// Rewrites the specified document context.
        /// </summary>
        /// <param name="document">The original document.</param>
        /// <param name="rewriter">The rewriter.</param>
        /// <returns></returns>
        Result<DocumentContext> Rewrite(DocumentContext document, IRewriter rewriter);
    }
}