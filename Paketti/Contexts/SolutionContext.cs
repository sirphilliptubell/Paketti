using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Paketti.Contexts
{
    public class SolutionContext
    {
        public Workspace Workspace { get; }
        public ProjectContext ProjectContext { get; }

        private SolutionContext(Workspace workspace, ProjectContext projectContext)
        {
            Workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
            ProjectContext = projectContext ?? throw new ArgumentNullException(nameof(projectContext));
        }

        public static Result<SolutionContext> Create(Workspace workspace)
        {
            if (workspace == null) throw new ArgumentException(nameof(workspace));

            var projectResult = GetProjectModel(workspace.CurrentSolution);

            if (projectResult.IsFailure)
                return Result.Fail<SolutionContext>(projectResult.Error);
            else
                return Result.Ok(new SolutionContext(workspace, projectResult.Value));
        }

        private static Result<ProjectContext> GetProjectModel(Solution solution)
            => GetProject(solution) //get the single project in the solution
            .OnSuccess(ProjectContext.Create); //convert to a context

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
    }
}