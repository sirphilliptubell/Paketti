using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Paketti.Contexts
{
    /// <summary>
    /// The contextual information for a Solution.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class SolutionContext
    {
        /// <summary>
        /// Gets the project's context.
        /// </summary>
        /// <value>
        /// The project's context.
        /// </value>
        public ProjectContext ProjectContext { get; }

        /// <summary>
        /// Gets the workspace of the project.
        /// </summary>
        /// <value>
        /// The workspace of the project.
        /// </value>
        public Workspace Workspace { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionContext"/> class.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <param name="projectContext">The project's context.</param>
        /// <exception cref="System.ArgumentNullException">
        /// workspace
        /// or
        /// projectContext
        /// </exception>
        private SolutionContext(Workspace workspace, ProjectContext projectContext)
        {
            Workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
            ProjectContext = projectContext ?? throw new ArgumentNullException(nameof(projectContext));
        }

        /// <summary>
        /// Tries to create a SolutionContext for the specified workspace.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">workspace</exception>
        public static Result<SolutionContext> Create(Workspace workspace)
        {
            if (workspace == null) throw new ArgumentException(nameof(workspace));

            var projectResult = GetProjectContext(workspace.CurrentSolution);

            if (projectResult.IsFailure)
                return Result.Fail<SolutionContext>(projectResult.Error);
            else
                return Result.Ok(new SolutionContext(workspace, projectResult.Value));
        }

        /// <summary>
        /// Gets the project for the solution.
        /// Fails if there isn't exactly one project in the solution.
        /// </summary>
        /// <param name="solution">The solution.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">solution</exception>
        private static Result<Project> GetProject(Solution solution)
        {
            if (solution == null) throw new ArgumentNullException(nameof(solution));

            var projects =
                solution
                .Projects
                .Take(2)
                .ToList();

            if (projects.Count == 2)
                return Result.Fail<Project>("Only one project is currently supported");
            if (projects.Count == 0)
                return Result.Fail<Project>("Workspace has no projects");

            return Result.Ok(projects.First());
        }

        /// <summary>
        /// Gets the project model.
        /// </summary>
        /// <param name="solution">The solution.</param>
        /// <returns></returns>
        private static Result<ProjectContext> GetProjectContext(Solution solution)
            => GetProject(solution) //get the single project in the solution
            .OnSuccess(ProjectContext.Create); //convert to a context

        private string DebuggerDisplay
            => ToString();
    }
}